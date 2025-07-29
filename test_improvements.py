#!/usr/bin/env python3
"""
개선된 Hipot AI Analyzer 테스트 스크립트
모든 개선사항의 기능성을 검증합니다.
"""

import sys
import time
import asyncio
import numpy as np
import pandas as pd
from datetime import datetime

# 개선된 AI 분석기 임포트
try:
    from hipot_ai_analyzer import HipotAIAnalyzer, create_sample_data
    print("✓ AI Analyzer 임포트 성공")
except ImportError as e:
    print(f"✗ AI Analyzer 임포트 실패: {e}")
    sys.exit(1)

async def test_ai_analyzer_performance():
    """AI 분석기 성능 테스트"""
    print("\n=== AI 분석기 성능 테스트 ===")
    
    try:
        # 분석기 초기화
        start_time = time.time()
        analyzer = HipotAIAnalyzer()
        analyzer.initialize_model()
        init_time = time.time() - start_time
        print(f"✓ 분석기 초기화: {init_time:.3f}초")
        
        # 훈련 데이터 생성 및 훈련
        start_time = time.time()
        sample_data = [create_sample_data() for _ in range(3)]
        training_results = analyzer.train_reference_model(sample_data)
        train_time = time.time() - start_time
        print(f"✓ 모델 훈련: {train_time:.3f}초 (최종 손실: {training_results['final_loss']:.6f})")
        
        # 분석 성능 테스트
        test_data = create_sample_data()
        start_time = time.time()
        analysis_result = analyzer.analyze_test_session(test_data)
        analyze_time = time.time() - start_time
        print(f"✓ 데이터 분석: {analyze_time:.3f}초")
        print(f"  - 정확도: {analysis_result['accuracy_metrics']['overall_accuracy']:.3f}")
        print(f"  - 불합률: {analysis_result['defect_metrics']['overall_defect_rate']:.1f}%")
        
        # 메모리 효율성 테스트 (대용량 데이터)
        large_data = pd.DataFrame({
            'time': np.linspace(0, 100, 15000),
            'voltage': np.random.normal(1000, 50, 15000),
            'current': np.random.normal(0.001, 0.0001, 15000),
            'resistance': np.random.normal(1e9, 1e8, 15000)
        })
        
        start_time = time.time()
        large_result = analyzer.analyze_test_session(large_data)
        large_analyze_time = time.time() - start_time
        print(f"✓ 대용량 데이터 분석 (15,000 포인트): {large_analyze_time:.3f}초")
        
        return True
        
    except Exception as e:
        print(f"✗ AI 분석기 테스트 실패: {e}")
        return False

def test_flask_api_imports():
    """Flask API 임포트 테스트"""
    print("\n=== Flask API 임포트 테스트 ===")
    
    try:
        from flask_api import app, initialize_analyzer
        print("✓ Flask API 임포트 성공")
        
        # 분석기 초기화 테스트
        if initialize_analyzer():
            print("✓ Flask 분석기 초기화 성공")
        else:
            print("✗ Flask 분석기 초기화 실패")
            return False
            
        return True
        
    except ImportError as e:
        print(f"✗ Flask API 임포트 실패: {e}")
        return False
    except Exception as e:
        print(f"✗ Flask API 테스트 실패: {e}")
        return False

def test_data_processing_efficiency():
    """데이터 처리 효율성 테스트"""
    print("\n=== 데이터 처리 효율성 테스트 ===")
    
    try:
        from hipot_ai_analyzer import HipotDataPreprocessor
        
        preprocessor = HipotDataPreprocessor()
        
        # 소규모 데이터 테스트
        small_data = create_sample_data()
        start_time = time.time()
        processed_small = preprocessor.preprocess(small_data)
        small_time = time.time() - start_time
        print(f"✓ 소규모 데이터 전처리 ({len(small_data)} 포인트): {small_time:.3f}초")
        
        # 대규모 데이터 테스트
        large_data = pd.DataFrame({
            'time': np.linspace(0, 100, 12000),
            'voltage': np.random.normal(1000, 50, 12000),
            'current': np.random.normal(0.001, 0.0001, 12000),
            'resistance': np.random.normal(1e9, 1e8, 12000)
        })
        
        start_time = time.time()
        processed_large = preprocessor.preprocess(large_data)
        large_time = time.time() - start_time
        print(f"✓ 대규모 데이터 전처리 ({len(large_data)} 포인트): {large_time:.3f}초")
        
        # 성능 비교
        efficiency_ratio = (len(large_data) / len(small_data)) / (large_time / small_time)
        print(f"✓ 처리 효율성 비율: {efficiency_ratio:.2f}x")
        
        return True
        
    except Exception as e:
        print(f"✗ 데이터 처리 효율성 테스트 실패: {e}")
        return False

def test_memory_optimization():
    """메모리 최적화 테스트"""
    print("\n=== 메모리 최적화 테스트 ===")
    
    try:
        import psutil
        import gc
        
        process = psutil.Process()
        initial_memory = process.memory_info().rss / 1024 / 1024  # MB
        
        # 분석기 생성 및 사용
        analyzer = HipotAIAnalyzer()
        analyzer.initialize_model()
        
        # 여러 번의 분석 수행
        for i in range(5):
            test_data = create_sample_data()
            if i == 0:
                # 첫 번째만 훈련
                sample_data = [create_sample_data() for _ in range(2)]
                analyzer.train_reference_model(sample_data)
            
            result = analyzer.analyze_test_session(test_data)
        
        # 가비지 컬렉션 수행
        gc.collect()
        
        final_memory = process.memory_info().rss / 1024 / 1024  # MB
        memory_increase = final_memory - initial_memory
        
        print(f"✓ 초기 메모리: {initial_memory:.1f} MB")
        print(f"✓ 최종 메모리: {final_memory:.1f} MB")
        print(f"✓ 메모리 증가: {memory_increase:.1f} MB")
        
        if memory_increase < 200:  # 200MB 미만 증가는 허용
            print("✓ 메모리 최적화 양호")
            return True
        else:
            print("⚠ 메모리 사용량이 높습니다")
            return False
            
    except ImportError:
        print("⚠ psutil이 설치되지 않아 메모리 테스트를 건너뜁니다")
        return True
    except Exception as e:
        print(f"✗ 메모리 최적화 테스트 실패: {e}")
        return False

async def main():
    """메인 테스트 함수"""
    print("🔧 Hipot Tester 개선사항 검증 시작")
    print(f"📅 테스트 시간: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    
    test_results = []
    
    # 각 테스트 실행
    test_results.append(await test_ai_analyzer_performance())
    test_results.append(test_flask_api_imports())
    test_results.append(test_data_processing_efficiency())
    test_results.append(test_memory_optimization())
    
    # 결과 요약
    print("\n" + "="*50)
    print("📊 테스트 결과 요약")
    print("="*50)
    
    passed = sum(test_results)
    total = len(test_results)
    
    print(f"통과: {passed}/{total} ({passed/total*100:.1f}%)")
    
    if passed == total:
        print("🎉 모든 테스트 통과! 개선사항이 성공적으로 적용되었습니다.")
        return 0
    else:
        print("⚠️ 일부 테스트 실패. 코드를 검토해주세요.")
        return 1

if __name__ == "__main__":
    exit_code = asyncio.run(main())
    sys.exit(exit_code)