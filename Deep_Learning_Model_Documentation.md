# Hipot 테스터 딥러닝 모델 설계 문서

## 1. 프로젝트 개요

### 1.1 현재 시스템 분석
- **시스템 유형**: Hipot (High Potential) 전기 안전 테스터
- **측정 모드**: AC, DC, IR (절연저항)
- **지원 장비**: Chroma 1903X, 1905X, 11210, 11210K 시리즈
- **데이터 구조**: 시간, 전압, 전류, 저항 측정값

### 1.2 데이터 추출 패턴
현재 시스템에서 한 테이블당 추출하는 주요 데이터:
```csharp
public class DataModel {
    public double Time { get; set; }        // 측정 시간 (초)
    public double Voltage { get; set; }     // 전압 (V)
    public double Current { get; set; }     // 전류 (A)
    public double Resistance { get; set; }  // 저항 (Ω)
}
```

## 2. 딥러닝 모델 아키텍처 설계

### 2.1 모델 목적
1. **기준 모델 생성**: 정상적인 테스트 데이터 패턴 학습
2. **정확도 평가**: 새로운 테스트 데이터와 기준 모델 비교
3. **불합률 계산**: 이상 데이터 탐지 및 분류

### 2.2 모델 구조

#### 2.2.1 데이터 전처리 파이프라인
```python
class HipotDataPreprocessor:
    def __init__(self):
        self.scaler = StandardScaler()
        self.outlier_detector = IsolationForest(contamination=0.1)
        
    def preprocess(self, raw_data):
        # 1. 결측값 처리
        # 2. 이상치 제거
        # 3. 정규화
        # 4. 시계열 특성 추출
        pass
```

#### 2.2.2 기준 모델 (Reference Model) 아키텍처
```python
class HipotReferenceModel(nn.Module):
    def __init__(self, input_dim=4, hidden_dim=128, num_layers=3):
        super().__init__()
        
        # LSTM for temporal patterns
        self.lstm = nn.LSTM(input_dim, hidden_dim, num_layers, 
                           batch_first=True, dropout=0.2)
        
        # Autoencoder for anomaly detection
        self.encoder = nn.Sequential(
            nn.Linear(hidden_dim, 64),
            nn.ReLU(),
            nn.Linear(64, 32),
            nn.ReLU(),
            nn.Linear(32, 16)
        )
        
        self.decoder = nn.Sequential(
            nn.Linear(16, 32),
            nn.ReLU(),
            nn.Linear(32, 64),
            nn.ReLU(),
            nn.Linear(64, hidden_dim)
        )
        
        # Classification head
        self.classifier = nn.Sequential(
            nn.Linear(16, 32),
            nn.ReLU(),
            nn.Dropout(0.3),
            nn.Linear(32, 5)  # Valid, Error, OutOfRange, Critical, Dead
        )
```

#### 2.2.3 정확도 및 불합률 계산 모델
```python
class AccuracyDefectCalculator:
    def __init__(self, reference_model):
        self.reference_model = reference_model
        self.threshold_config = {
            'reconstruction_threshold': 0.1,
            'classification_confidence': 0.8,
            'temporal_deviation': 0.05
        }
    
    def calculate_accuracy(self, test_data, reference_data):
        # 테스트 데이터와 기준 모델 간 유사도 계산
        pass
    
    def calculate_defect_rate(self, test_results):
        # 불합률 계산 및 분류
        pass
```

### 2.3 그래프 생성 및 비교 시스템

#### 2.3.1 동적 그래프 생성기
```python
class HipotGraphGenerator:
    def __init__(self):
        self.plot_configs = {
            'voltage_current': {'x': 'time', 'y': ['voltage', 'current']},
            'resistance_time': {'x': 'time', 'y': 'resistance'},
            'vi_characteristic': {'x': 'voltage', 'y': 'current'},
            'anomaly_heatmap': {'type': 'heatmap'},
            'statistical_distribution': {'type': 'histogram'}
        }
    
    def generate_comparison_plots(self, test_data, reference_data):
        # 1. 실시간 측정 그래프
        # 2. 기준 모델과의 비교 그래프
        # 3. 이상 탐지 히트맵
        # 4. 통계적 분포 비교
        pass
```

## 3. 구현 세부사항

### 3.1 데이터 분류 체계
```python
from enum import Enum

class DataClassification(Enum):
    VALID = "정상"
    ERROR = "오류"
    OUT_OF_RANGE = "범위 초과"
    CRITICAL = "치명적"
    DEAD = "측정 불가"

class TestResult(Enum):
    PASS = "합격"
    HIGH_FAIL = "과전류 불합격"
    LOW_FAIL = "저전류 불합격"
    OUTPUT_FAIL = "출력 불합격"
```

### 3.2 실시간 모니터링 시스템
```python
class RealTimeMonitor:
    def __init__(self, model, graph_generator):
        self.model = model
        self.graph_generator = graph_generator
        self.alert_system = AlertSystem()
        
    def process_measurement(self, measurement_data):
        # 1. 실시간 데이터 전처리
        # 2. 모델 예측
        # 3. 그래프 업데이트
        # 4. 이상 감지 시 알림
        pass
```

### 3.3 학습 데이터 관리
```python
class TrainingDataManager:
    def __init__(self, data_path):
        self.data_path = data_path
        self.validation_split = 0.2
        
    def prepare_training_data(self):
        # 1. 과거 테스트 세션 데이터 수집
        # 2. 정상/비정상 라벨링
        # 3. 교차 검증 데이터셋 생성
        pass
    
    def augment_data(self, data):
        # 데이터 증강 기법 적용
        # - 노이즈 추가
        # - 시간 왜곡
        # - 전압/전류 스케일링
        pass
```

## 4. 모델 학습 및 평가

### 4.1 학습 전략
```python
class HipotModelTrainer:
    def __init__(self, model, config):
        self.model = model
        self.config = config
        self.criterion = {
            'reconstruction': nn.MSELoss(),
            'classification': nn.CrossEntropyLoss(),
            'temporal': TemporalConsistencyLoss()
        }
    
    def train(self, train_loader, val_loader):
        # 1. Multi-task learning
        # 2. Progressive training
        # 3. Early stopping
        # 4. Model checkpointing
        pass
```

### 4.2 평가 메트릭
```python
class ModelEvaluator:
    def __init__(self):
        self.metrics = {
            'accuracy': accuracy_score,
            'precision': precision_score,
            'recall': recall_score,
            'f1': f1_score,
            'auc_roc': roc_auc_score,
            'reconstruction_error': mean_squared_error
        }
    
    def comprehensive_evaluation(self, model, test_data):
        # 1. 분류 성능 평가
        # 2. 이상 탐지 성능 평가
        # 3. 시간적 일관성 평가
        # 4. 도메인 특화 메트릭 평가
        pass
```

## 5. 기준 모델 생성 프로세스

### 5.1 정상 패턴 학습
```python
class ReferenceModelBuilder:
    def __init__(self):
        self.pattern_extractors = {
            'voltage_ramp': VoltageRampExtractor(),
            'current_response': CurrentResponseExtractor(),
            'resistance_stability': ResistanceStabilityExtractor(),
            'temporal_consistency': TemporalConsistencyExtractor()
        }
    
    def build_reference_model(self, historical_data):
        # 1. 정상 데이터 필터링
        # 2. 패턴 추출 및 클러스터링
        # 3. 기준 프로파일 생성
        # 4. 허용 오차 범위 설정
        pass
```

### 5.2 적응적 기준 모델 업데이트
```python
class AdaptiveReferenceUpdater:
    def __init__(self, reference_model):
        self.reference_model = reference_model
        self.update_threshold = 0.95  # 신뢰도 임계값
        
    def update_reference(self, new_valid_data):
        # 1. 새로운 정상 데이터 검증
        # 2. 기존 모델과의 일관성 확인
        # 3. 점진적 모델 업데이트
        # 4. 성능 검증
        pass
```

## 6. 정확도 및 불합률 계산

### 6.1 정확도 평가 알고리즘
```python
def calculate_test_accuracy(test_data, reference_model):
    """
    테스트 데이터의 정확도 계산
    
    Args:
        test_data: 측정된 테스트 데이터
        reference_model: 기준 모델
    
    Returns:
        accuracy_metrics: 정확도 관련 메트릭들
    """
    
    # 1. 패턴 유사도 계산
    pattern_similarity = calculate_pattern_similarity(test_data, reference_model)
    
    # 2. 통계적 일치도 계산
    statistical_match = calculate_statistical_match(test_data, reference_model)
    
    # 3. 시간적 일관성 평가
    temporal_consistency = evaluate_temporal_consistency(test_data)
    
    # 4. 종합 정확도 점수
    overall_accuracy = weighted_average([
        (pattern_similarity, 0.4),
        (statistical_match, 0.3),
        (temporal_consistency, 0.3)
    ])
    
    return {
        'overall_accuracy': overall_accuracy,
        'pattern_similarity': pattern_similarity,
        'statistical_match': statistical_match,
        'temporal_consistency': temporal_consistency
    }
```

### 6.2 불합률 계산 알고리즘
```python
def calculate_defect_rate(test_results, classification_model):
    """
    테스트 결과의 불합률 계산
    
    Args:
        test_results: 테스트 결과 리스트
        classification_model: 분류 모델
    
    Returns:
        defect_metrics: 불합률 관련 메트릭들
    """
    
    classifications = classification_model.predict(test_results)
    
    total_tests = len(test_results)
    defect_counts = {
        'error': sum(1 for c in classifications if c == DataClassification.ERROR),
        'out_of_range': sum(1 for c in classifications if c == DataClassification.OUT_OF_RANGE),
        'critical': sum(1 for c in classifications if c == DataClassification.CRITICAL),
        'dead': sum(1 for c in classifications if c == DataClassification.DEAD)
    }
    
    defect_rates = {
        defect_type: count / total_tests * 100
        for defect_type, count in defect_counts.items()
    }
    
    overall_defect_rate = sum(defect_counts.values()) / total_tests * 100
    
    return {
        'overall_defect_rate': overall_defect_rate,
        'defect_breakdown': defect_rates,
        'total_tests': total_tests,
        'pass_rate': (total_tests - sum(defect_counts.values())) / total_tests * 100
    }
```

## 7. 그래프 비교 및 시각화

### 7.1 실시간 비교 그래프
```python
class ComparisonGraphGenerator:
    def __init__(self):
        self.fig_configs = {
            'voltage_current_comparison': {
                'subplots': 2,
                'shared_x': True,
                'figsize': (12, 8)
            },
            'deviation_heatmap': {
                'figsize': (10, 6),
                'colormap': 'RdYlBu'
            }
        }
    
    def create_real_time_comparison(self, current_data, reference_data):
        """실시간 측정 데이터와 기준 데이터 비교 그래프 생성"""
        
        fig, axes = plt.subplots(2, 2, figsize=(15, 10))
        
        # 전압-시간 비교
        axes[0, 0].plot(current_data['time'], current_data['voltage'], 
                       label='Current Test', color='blue', alpha=0.7)
        axes[0, 0].plot(reference_data['time'], reference_data['voltage'], 
                       label='Reference', color='red', linestyle='--')
        axes[0, 0].set_title('Voltage vs Time Comparison')
        axes[0, 0].legend()
        
        # 전류-시간 비교
        axes[0, 1].plot(current_data['time'], current_data['current'], 
                       label='Current Test', color='blue', alpha=0.7)
        axes[0, 1].plot(reference_data['time'], reference_data['current'], 
                       label='Reference', color='red', linestyle='--')
        axes[0, 1].set_title('Current vs Time Comparison')
        axes[0, 1].legend()
        
        # 저항-시간 비교
        axes[1, 0].semilogy(current_data['time'], current_data['resistance'], 
                           label='Current Test', color='blue', alpha=0.7)
        axes[1, 0].semilogy(reference_data['time'], reference_data['resistance'], 
                           label='Reference', color='red', linestyle='--')
        axes[1, 0].set_title('Resistance vs Time Comparison (Log Scale)')
        axes[1, 0].legend()
        
        # 편차 히트맵
        deviation_matrix = self.calculate_deviation_matrix(current_data, reference_data)
        im = axes[1, 1].imshow(deviation_matrix, aspect='auto', cmap='RdYlBu')
        axes[1, 1].set_title('Deviation Heatmap')
        plt.colorbar(im, ax=axes[1, 1])
        
        plt.tight_layout()
        return fig
```

### 7.2 통계적 분석 그래프
```python
def create_statistical_analysis_plots(test_data, reference_data):
    """통계적 분석 및 분포 비교 그래프"""
    
    fig, axes = plt.subplots(2, 3, figsize=(18, 12))
    
    # 히스토그램 비교
    for i, param in enumerate(['voltage', 'current', 'resistance']):
        axes[0, i].hist(test_data[param], bins=50, alpha=0.7, 
                       label='Test Data', color='blue')
        axes[0, i].hist(reference_data[param], bins=50, alpha=0.7, 
                       label='Reference Data', color='red')
        axes[0, i].set_title(f'{param.capitalize()} Distribution')
        axes[0, i].legend()
    
    # QQ Plot
    for i, param in enumerate(['voltage', 'current', 'resistance']):
        stats.probplot(test_data[param], dist="norm", plot=axes[1, i])
        axes[1, i].set_title(f'{param.capitalize()} Q-Q Plot')
    
    plt.tight_layout()
    return fig
```

## 8. 실제 구현 코드 구조

### 8.1 메인 애플리케이션 클래스
```python
class HipotAIAnalyzer:
    def __init__(self, config_path):
        self.config = self.load_config(config_path)
        self.preprocessor = HipotDataPreprocessor()
        self.reference_model = self.load_reference_model()
        self.graph_generator = HipotGraphGenerator()
        self.monitor = RealTimeMonitor(self.reference_model, self.graph_generator)
        
    def analyze_test_session(self, test_data):
        """완전한 테스트 세션 분석"""
        
        # 1. 데이터 전처리
        processed_data = self.preprocessor.preprocess(test_data)
        
        # 2. 기준 모델과 비교
        accuracy_metrics = self.calculate_test_accuracy(processed_data)
        
        # 3. 불합률 계산
        defect_metrics = self.calculate_defect_rate(processed_data)
        
        # 4. 그래프 생성
        comparison_plots = self.graph_generator.create_comparison_plots(
            processed_data, self.reference_model.reference_data
        )
        
        # 5. 결과 리포트 생성
        report = self.generate_analysis_report(
            accuracy_metrics, defect_metrics, comparison_plots
        )
        
        return report
    
    def update_reference_model(self, new_data):
        """기준 모델 업데이트"""
        if self.validate_new_data(new_data):
            self.reference_model.update(new_data)
            self.save_reference_model()
```

## 9. 배포 및 통합

### 9.1 C# 연동을 위한 Python API
```python
# hipot_ai_api.py
from flask import Flask, request, jsonify
import numpy as np
import json

app = Flask(__name__)
analyzer = HipotAIAnalyzer('config.json')

@app.route('/analyze', methods=['POST'])
def analyze_data():
    """C# 애플리케이션에서 호출할 분석 API"""
    try:
        data = request.json
        test_data = {
            'time': data['Time'],
            'voltage': data['Voltage'], 
            'current': data['Current'],
            'resistance': data['Resistance']
        }
        
        result = analyzer.analyze_test_session(test_data)
        return jsonify(result)
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/update_reference', methods=['POST'])
def update_reference():
    """기준 모델 업데이트 API"""
    try:
        data = request.json
        analyzer.update_reference_model(data)
        return jsonify({'status': 'success'})
        
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(host='127.0.0.1', port=5000)
```

### 9.2 C# 측 통합 코드 (DataManager.cs 확장)
```csharp
// C# 측에서 Python API 호출
public class AIAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://127.0.0.1:5000";
    
    public AIAnalysisService()
    {
        _httpClient = new HttpClient();
    }
    
    public async Task<AIAnalysisResult> AnalyzeTestDataAsync(ObservableCollection<DataModel> testData)
    {
        var requestData = new
        {
            Time = testData.Select(d => d.Time).ToArray(),
            Voltage = testData.Select(d => d.Voltage).ToArray(),
            Current = testData.Select(d => d.Current).ToArray(),
            Resistance = testData.Select(d => d.Resistance).ToArray()
        };
        
        var json = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_apiBaseUrl}/analyze", content);
        var responseJson = await response.Content.ReadAsStringAsync();
        
        return JsonConvert.DeserializeObject<AIAnalysisResult>(responseJson);
    }
}

public class AIAnalysisResult
{
    public double OverallAccuracy { get; set; }
    public double PatternSimilarity { get; set; }
    public double StatisticalMatch { get; set; }
    public double TemporalConsistency { get; set; }
    public double OverallDefectRate { get; set; }
    public Dictionary<string, double> DefectBreakdown { get; set; }
    public double PassRate { get; set; }
    public string[] GraphPaths { get; set; }
}
```

## 10. 설정 및 실행

### 10.1 환경 설정
```bash
# Python 환경 설정
pip install torch torchvision torchaudio
pip install scikit-learn pandas numpy matplotlib seaborn
pip install flask requests
pip install plotly dash  # 선택적: 웹 기반 대시보드용

# 프로젝트 구조
hipot_ai_analyzer/
├── models/
│   ├── reference_model.py
│   ├── accuracy_calculator.py
│   └── defect_detector.py
├── data/
│   ├── preprocessor.py
│   └── validator.py
├── visualization/
│   ├── graph_generator.py
│   └── dashboard.py
├── api/
│   └── flask_api.py
├── config/
│   └── model_config.json
└── main.py
```

### 10.2 실행 명령
```bash
# AI 분석 서버 시작
python hipot_ai_analyzer/api/flask_api.py

# 기준 모델 훈련 (최초 실행 시)
python hipot_ai_analyzer/train_reference_model.py --data_path ./historical_data

# 실시간 모니터링 대시보드 (선택적)
python hipot_ai_analyzer/visualization/dashboard.py
```

## 11. 결론

이 딥러닝 모델은 Hipot 테스터의 측정 데이터를 실시간으로 분석하여:

1. **정확한 기준 모델**: 과거 정상 데이터로부터 학습된 기준 패턴
2. **실시간 정확도 평가**: 현재 테스트와 기준 모델 간 유사도 분석
3. **자동 불합률 계산**: 다양한 불량 유형 자동 분류 및 통계
4. **직관적 시각화**: 실시간 그래프 및 비교 차트
5. **적응적 학습**: 새로운 정상 데이터로 지속적인 모델 개선

현재 C# WPF 애플리케이션과 완벽하게 통합되어 기존 워크플로우를 방해하지 않으면서도 고급 AI 분석 기능을 제공합니다.