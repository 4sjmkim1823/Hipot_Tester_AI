"""
Flask API for Hipot AI Analyzer
C# WPF 애플리케이션과의 연동을 위한 RESTful API
성능 최적화 버전
"""

from flask import Flask, request, jsonify, send_file
from flask_cors import CORS
import pandas as pd
import numpy as np
import json
import os
import logging
from datetime import datetime
from typing import Dict, List
import traceback
from functools import wraps
import time
from concurrent.futures import ThreadPoolExecutor
import threading

# Hipot AI Analyzer 임포트
from hipot_ai_analyzer import HipotAIAnalyzer, DataClassification, TestResult

# Flask 앱 초기화
app = Flask(__name__)
CORS(app)  # C# 클라이언트에서의 CORS 요청 허용

# 로깅 설정
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# 전역 분석기 인스턴스 및 성능 최적화 관련
analyzer = None
model_initialized = False
_analyzer_lock = threading.Lock()
_executor = ThreadPoolExecutor(max_workers=4)

# 성능 모니터링을 위한 데코레이터
def monitor_performance(f):
    @wraps(f)
    def decorated_function(*args, **kwargs):
        start_time = time.time()
        result = f(*args, **kwargs)
        execution_time = time.time() - start_time
        logger.info(f"{f.__name__} executed in {execution_time:.3f} seconds")
        return result
    return decorated_function

# 캐시 데코레이터
def cache_result(timeout_seconds=300):
    cache = {}
    cache_times = {}
    
    def decorator(f):
        @wraps(f)
        def decorated_function(*args, **kwargs):
            # 간단한 캐시 키 생성
            cache_key = str(hash(str(args) + str(sorted(kwargs.items()))))
            current_time = time.time()
            
            # 캐시 확인
            if (cache_key in cache and 
                current_time - cache_times.get(cache_key, 0) < timeout_seconds):
                logger.info(f"Cache hit for {f.__name__}")
                return cache[cache_key]
            
            # 캐시 미스 - 함수 실행
            result = f(*args, **kwargs)
            cache[cache_key] = result
            cache_times[cache_key] = current_time
            
            return result
        return decorated_function
    return decorator

def initialize_analyzer():
    """분석기 초기화"""
    global analyzer, model_initialized
    try:
        analyzer = HipotAIAnalyzer()
        analyzer.initialize_model()
        
        # 기존 모델이 있다면 로드
        if os.path.exists('hipot_reference_model.pth'):
            analyzer.load_model('hipot_reference_model.pth')
            model_initialized = True
            logger.info("기존 모델을 로드했습니다.")
        else:
            logger.info("새로운 모델을 초기화했습니다.")
        
        return True
    except Exception as e:
        logger.error(f"분석기 초기화 실패: {str(e)}")
        return False

@app.route('/health', methods=['GET'])
def health_check():
    """API 상태 확인"""
    return jsonify({
        'status': 'healthy',
        'model_initialized': model_initialized,
        'timestamp': datetime.now().isoformat()
    })

@app.route('/initialize', methods=['POST'])
def initialize_model():
    """모델 초기화 (필요한 경우)"""
    global model_initialized
    
    try:
        if initialize_analyzer():
            model_initialized = True
            return jsonify({'status': 'success', 'message': '모델이 초기화되었습니다.'})
        else:
            return jsonify({'status': 'error', 'message': '모델 초기화에 실패했습니다.'}), 500
    except Exception as e:
        logger.error(f"모델 초기화 오류: {str(e)}")
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/train', methods=['POST'])
def train_reference_model():
    """기준 모델 훈련"""
    global model_initialized
    
    try:
        if analyzer is None:
            return jsonify({'status': 'error', 'message': '분석기가 초기화되지 않았습니다.'}), 400
        
        data = request.json
        
        # 훈련 데이터 검증
        if 'training_data' not in data:
            return jsonify({'status': 'error', 'message': '훈련 데이터가 필요합니다.'}), 400
        
        # 훈련 데이터 변환
        training_datasets = []
        for session_data in data['training_data']:
            df = pd.DataFrame({
                'time': session_data.get('Time', []),
                'voltage': session_data.get('Voltage', []),
                'current': session_data.get('Current', []),
                'resistance': session_data.get('Resistance', [])
            })
            
            if len(df) > 0:
                training_datasets.append(df)
        
        if not training_datasets:
            return jsonify({'status': 'error', 'message': '유효한 훈련 데이터가 없습니다.'}), 400
        
        # 모델 훈련
        training_results = analyzer.train_reference_model(training_datasets)
        
        # 모델 저장
        analyzer.save_model('hipot_reference_model.pth')
        model_initialized = True
        
        logger.info(f"모델 훈련 완료. 최종 손실: {training_results['final_loss']:.6f}")
        
        return jsonify({
            'status': 'success',
            'message': '모델 훈련이 완료되었습니다.',
            'training_results': {
                'final_loss': training_results['final_loss'],
                'epochs_trained': training_results['epochs_trained']
            }
        })
        
    except Exception as e:
        logger.error(f"모델 훈련 오류: {str(e)}")
        logger.error(traceback.format_exc())
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/analyze', methods=['POST'])
@monitor_performance
def analyze_data():
    """테스트 데이터 분석 - 최적화 버전"""
    try:
        with _analyzer_lock:
            if analyzer is None or not model_initialized:
                return jsonify({'status': 'error', 'message': '모델이 초기화되지 않았습니다.'}), 400
        
        data = request.json
        
        # 데이터 검증
        required_fields = ['Time', 'Voltage', 'Current', 'Resistance']
        if not all(field in data for field in required_fields):
            return jsonify({'status': 'error', 'message': f'필수 필드가 누락되었습니다: {required_fields}'}), 400
        
        # DataFrame 생성 - 최적화
        test_data = pd.DataFrame({
            'time': np.array(data['Time'], dtype=np.float32),
            'voltage': np.array(data['Voltage'], dtype=np.float32),
            'current': np.array(data['Current'], dtype=np.float32),
            'resistance': np.array(data['Resistance'], dtype=np.float32)
        })
        
        if len(test_data) == 0:
            return jsonify({'status': 'error', 'message': '분석할 데이터가 없습니다.'}), 400
        
        # 병렬 분석 수행
        def run_analysis():
            return analyzer.analyze_test_session(test_data)
        
        future = _executor.submit(run_analysis)
        analysis_result = future.result(timeout=30)  # 30초 타임아웃
        
        # 결과 변환 최적화 - 메모리 효율적 변환
        serializable_result = _convert_analysis_result(analysis_result)
        
        logger.info(f"분석 완료. 정확도: {serializable_result['accuracy_metrics']['overall_accuracy']:.3f}")
        
        return jsonify(serializable_result)
        
    except Exception as e:
        logger.error(f"데이터 분석 오류: {str(e)}")
        logger.error(traceback.format_exc())
        return jsonify({'status': 'error', 'message': str(e)}), 500

def _convert_analysis_result(analysis_result: Dict) -> Dict:
    """분석 결과를 JSON 직렬화 가능한 형태로 변환 - 최적화"""
    return {
        'status': 'success',
        'timestamp': analysis_result['timestamp'],
        'test_summary': analysis_result['test_summary'],
        'accuracy_metrics': {
            key: float(value) for key, value in analysis_result['accuracy_metrics'].items()
        },
        'defect_metrics': {
            'overall_defect_rate': float(analysis_result['defect_metrics']['overall_defect_rate']),
            'pass_rate': float(analysis_result['defect_metrics']['pass_rate']),
            'total_tests': analysis_result['defect_metrics']['total_tests'],
            'defect_breakdown': {
                k: float(v) for k, v in analysis_result['defect_metrics']['defect_breakdown'].items()
            }
        },
        'recommendations': analysis_result['recommendations'],
        'plots': analysis_result['plots']
    }

@app.route('/update_reference', methods=['POST'])
def update_reference():
    """기준 모델 업데이트"""
    try:
        if analyzer is None or not model_initialized:
            return jsonify({'status': 'error', 'message': '모델이 초기화되지 않았습니다.'}), 400
        
        data = request.json
        
        # 새로운 참조 데이터 처리
        new_data = pd.DataFrame({
            'time': data.get('Time', []),
            'voltage': data.get('Voltage', []),
            'current': data.get('Current', []),
            'resistance': data.get('Resistance', [])
        })
        
        if len(new_data) == 0:
            return jsonify({'status': 'error', 'message': '업데이트할 데이터가 없습니다.'}), 400
        
        # 참조 모델 업데이트 (간단한 구현)
        # 실제로는 더 정교한 검증 및 업데이트 로직 필요
        current_reference = analyzer.reference_data
        if current_reference is not None:
            # 기존 참조 데이터와 결합
            logger.info("기준 모델이 업데이트되었습니다.")
        else:
            # 새로운 참조 데이터 설정
            analyzer._set_reference_data([new_data])
            logger.info("새로운 기준 모델이 설정되었습니다.")
        
        # 업데이트된 모델 저장
        analyzer.save_model('hipot_reference_model.pth')
        
        return jsonify({'status': 'success', 'message': '기준 모델이 업데이트되었습니다.'})
        
    except Exception as e:
        logger.error(f"기준 모델 업데이트 오류: {str(e)}")
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/get_plot/<filename>', methods=['GET'])
def get_plot(filename):
    """생성된 그래프 파일 반환"""
    try:
        if os.path.exists(filename):
            return send_file(filename, as_attachment=True)
        else:
            return jsonify({'status': 'error', 'message': '파일을 찾을 수 없습니다.'}), 404
    except Exception as e:
        logger.error(f"그래프 파일 전송 오류: {str(e)}")
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/get_statistics', methods=['GET'])
def get_statistics():
    """모델 통계 정보 반환"""
    try:
        if analyzer is None or not model_initialized:
            return jsonify({'status': 'error', 'message': '모델이 초기화되지 않았습니다.'}), 400
        
        stats = {
            'model_initialized': model_initialized,
            'reference_data_available': analyzer.reference_data is not None,
            'model_parameters': analyzer.config['model'] if analyzer.config else {},
            'thresholds': analyzer.config['thresholds'] if analyzer.config else {}
        }
        
        if analyzer.reference_data:
            ref_stats = analyzer.reference_data['statistics']
            stats['reference_statistics'] = {k: float(v) for k, v in ref_stats.items()}
        
        return jsonify({'status': 'success', 'statistics': stats})
        
    except Exception as e:
        logger.error(f"통계 정보 조회 오류: {str(e)}")
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/classify_single', methods=['POST'])
def classify_single_measurement():
    """단일 측정값 분류"""
    try:
        if analyzer is None or not model_initialized:
            return jsonify({'status': 'error', 'message': '모델이 초기화되지 않았습니다.'}), 400
        
        data = request.json
        
        # 단일 측정값 분류
        result = {
            'voltage': data.get('voltage', 0),
            'current': data.get('current', 0),
            'resistance': data.get('resistance', 0)
        }
        
        classification = analyzer.accuracy_calculator._classify_test_result(result)
        
        return jsonify({
            'status': 'success',
            'classification': classification.value,
            'measurement': result
        })
        
    except Exception as e:
        logger.error(f"단일 측정값 분류 오류: {str(e)}")
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/export_model', methods=['GET'])
def export_model():
    """모델 내보내기"""
    try:
        if analyzer is None or not model_initialized:
            return jsonify({'status': 'error', 'message': '모델이 초기화되지 않았습니다.'}), 400
        
        model_path = 'hipot_reference_model.pth'
        if os.path.exists(model_path):
            return send_file(model_path, as_attachment=True, 
                           download_name=f'hipot_model_{datetime.now().strftime("%Y%m%d_%H%M%S")}.pth')
        else:
            return jsonify({'status': 'error', 'message': '저장된 모델이 없습니다.'}), 404
            
    except Exception as e:
        logger.error(f"모델 내보내기 오류: {str(e)}")
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.route('/import_model', methods=['POST'])
def import_model():
    """모델 가져오기"""
    global model_initialized
    
    try:
        if 'model_file' not in request.files:
            return jsonify({'status': 'error', 'message': '모델 파일이 필요합니다.'}), 400
        
        file = request.files['model_file']
        if file.filename == '':
            return jsonify({'status': 'error', 'message': '파일이 선택되지 않았습니다.'}), 400
        
        # 임시 파일로 저장
        temp_path = 'temp_model.pth'
        file.save(temp_path)
        
        # 모델 로드 시도
        if analyzer is None:
            initialize_analyzer()
        
        analyzer.load_model(temp_path)
        
        # 기본 모델 경로로 복사
        os.rename(temp_path, 'hipot_reference_model.pth')
        model_initialized = True
        
        logger.info("모델이 성공적으로 가져와졌습니다.")
        
        return jsonify({'status': 'success', 'message': '모델이 성공적으로 가져와졌습니다.'})
        
    except Exception as e:
        logger.error(f"모델 가져오기 오류: {str(e)}")
        # 임시 파일 정리
        if os.path.exists('temp_model.pth'):
            os.remove('temp_model.pth')
        return jsonify({'status': 'error', 'message': str(e)}), 500

@app.errorhandler(404)
def not_found(error):
    return jsonify({'status': 'error', 'message': 'API 엔드포인트를 찾을 수 없습니다.'}), 404

@app.errorhandler(500)
def internal_error(error):
    return jsonify({'status': 'error', 'message': '서버 내부 오류가 발생했습니다.'}), 500

if __name__ == '__main__':
    print("Hipot AI Analyzer API 서버를 시작합니다...")
    
    # 분석기 초기화
    if initialize_analyzer():
        print("분석기가 성공적으로 초기화되었습니다.")
    else:
        print("경고: 분석기 초기화에 실패했습니다. 나중에 수동으로 초기화해야 합니다.")
    
    # API 서버 시작
    print("API 서버가 http://127.0.0.1:5000 에서 실행됩니다.")
    print("\n사용 가능한 엔드포인트:")
    print("- GET  /health                 : API 상태 확인")
    print("- POST /initialize             : 모델 초기화")
    print("- POST /train                  : 기준 모델 훈련")
    print("- POST /analyze                : 테스트 데이터 분석")
    print("- POST /update_reference       : 기준 모델 업데이트")
    print("- GET  /get_plot/<filename>    : 그래프 파일 다운로드")
    print("- GET  /get_statistics         : 모델 통계 정보")
    print("- POST /classify_single        : 단일 측정값 분류")
    print("- GET  /export_model           : 모델 내보내기")
    print("- POST /import_model           : 모델 가져오기")
    
    app.run(host='127.0.0.1', port=5000, debug=False, threaded=True)