"""
Hipot 테스터 딥러닝 분석 시스템
현재 C# WPF 애플리케이션과 통합되는 AI 분석 엔진
성능 최적화 버전
"""

import numpy as np
import pandas as pd
from typing import Dict, List, Tuple, Optional
from enum import Enum
from datetime import datetime
import json
import pickle
import warnings
from functools import lru_cache
from concurrent.futures import ThreadPoolExecutor
import threading

# 성능을 위한 지연 로딩
_matplotlib_loaded = False
_sklearn_loaded = False
_torch_loaded = False

warnings.filterwarnings('ignore')

def _load_matplotlib():
    """matplotlib 지연 로딩"""
    global _matplotlib_loaded
    if not _matplotlib_loaded:
        global plt, sns
        import matplotlib.pyplot as plt
        import seaborn as sns
        plt.style.use('seaborn-v0_8')
        _matplotlib_loaded = True

def _load_sklearn():
    """scikit-learn 지연 로딩"""
    global _sklearn_loaded
    if not _sklearn_loaded:
        global StandardScaler, MinMaxScaler, IsolationForest, DBSCAN
        global accuracy_score, precision_score, recall_score, f1_score, roc_auc_score, mean_squared_error
        from sklearn.preprocessing import StandardScaler, MinMaxScaler
        from sklearn.ensemble import IsolationForest
        from sklearn.metrics import accuracy_score, precision_score, recall_score, f1_score, roc_auc_score, mean_squared_error
        from sklearn.cluster import DBSCAN
        _sklearn_loaded = True

def _load_torch():
    """PyTorch 지연 로딩"""
    global _torch_loaded
    if not _torch_loaded:
        global torch, nn, optim, DataLoader, TensorDataset
        import torch
        import torch.nn as nn
        import torch.optim as optim
        from torch.utils.data import DataLoader, TensorDataset
        _torch_loaded = True

class DataClassification(Enum):
    """데이터 분류 열거형"""
    VALID = "정상"
    ERROR = "오류"
    OUT_OF_RANGE = "범위 초과"
    CRITICAL = "치명적"
    DEAD = "측정 불가"

class TestResult(Enum):
    """테스트 결과 열거형"""
    PASS = "합격"
    HIGH_FAIL = "과전류 불합격"
    LOW_FAIL = "저전류 불합격"
    OUTPUT_FAIL = "출력 불합격"

class HipotDataPreprocessor:
    """Hipot 테스터 데이터 전처리 클래스 - 성능 최적화 버전"""
    
    def __init__(self):
        _load_sklearn()
        self.scaler = StandardScaler()
        self.outlier_detector = IsolationForest(contamination=0.1, random_state=42, n_jobs=-1)
        self.is_fitted = False
        self._lock = threading.Lock()
        
        # 캐시된 통계값들
        self._cached_stats = {}
        
    def fit(self, data: pd.DataFrame):
        """전처리 파라미터 학습"""
        # 결측값 처리된 데이터로 스케일러 학습
        clean_data = self._handle_missing_values(data)
        self.scaler.fit(clean_data[['time', 'voltage', 'current', 'resistance']])
        self.outlier_detector.fit(clean_data[['voltage', 'current', 'resistance']])
        self.is_fitted = True
        
    def preprocess(self, data: pd.DataFrame) -> pd.DataFrame:
        """데이터 전처리 수행 - 병렬화 및 최적화"""
        with self._lock:
            if not self.is_fitted:
                self.fit(data)
        
        # 데이터 크기에 따라 처리 방식 결정
        if len(data) > 10000:
            return self._preprocess_large_data(data)
        else:
            return self._preprocess_small_data(data)
    
    def _preprocess_small_data(self, data: pd.DataFrame) -> pd.DataFrame:
        """소규모 데이터 전처리"""
        processed_data = self._handle_missing_values(data.copy())
        processed_data = self._remove_outliers(processed_data)
        processed_data = self._normalize_data(processed_data)
        processed_data = self._extract_temporal_features(processed_data)
        return processed_data
    
    def _preprocess_large_data(self, data: pd.DataFrame) -> pd.DataFrame:
        """대규모 데이터 병렬 전처리"""
        with ThreadPoolExecutor(max_workers=4) as executor:
            # 병렬로 처리할 작업들
            missing_future = executor.submit(self._handle_missing_values, data.copy())
            
            # 결측값 처리 완료 후 후속 작업
            processed_data = missing_future.result()
            
            # 나머지 작업들을 순차적으로 수행 (메모리 효율성을 위해)
            processed_data = self._remove_outliers(processed_data)
            processed_data = self._normalize_data(processed_data)
            processed_data = self._extract_temporal_features(processed_data)
            
        return processed_data
    
    @lru_cache(maxsize=32)
    def _get_interpolation_method(self, data_size: int) -> str:
        """데이터 크기에 따른 최적 보간 방법 결정"""
        return 'linear' if data_size < 1000 else 'nearest'
    
    def _handle_missing_values(self, data: pd.DataFrame) -> pd.DataFrame:
        """결측값 처리 - 최적화 버전"""
        if data.isnull().sum().sum() == 0:
            return data
            
        method = self._get_interpolation_method(len(data))
        data = data.interpolate(method=method, limit_direction='both')
        data = data.fillna(method='ffill').fillna(method='bfill')
        return data
    
    def _remove_outliers(self, data: pd.DataFrame) -> pd.DataFrame:
        """이상치 제거"""
        outlier_mask = self.outlier_detector.predict(data[['voltage', 'current', 'resistance']]) == 1
        return data[outlier_mask]
    
    def _normalize_data(self, data: pd.DataFrame) -> pd.DataFrame:
        """데이터 정규화"""
        normalized_features = self.scaler.transform(data[['time', 'voltage', 'current', 'resistance']])
        data[['time_norm', 'voltage_norm', 'current_norm', 'resistance_norm']] = normalized_features
        return data
    
    def _extract_temporal_features(self, data: pd.DataFrame) -> pd.DataFrame:
        """시계열 특성 추출"""
        # 1차 미분 (변화율)
        data['voltage_diff'] = data['voltage'].diff()
        data['current_diff'] = data['current'].diff()
        data['resistance_diff'] = data['resistance'].diff()
        
        # 이동평균 (노이즈 제거)
        window_size = min(5, len(data) // 10)
        if window_size > 1:
            data['voltage_ma'] = data['voltage'].rolling(window=window_size).mean()
            data['current_ma'] = data['current'].rolling(window=window_size).mean()
            data['resistance_ma'] = data['resistance'].rolling(window=window_size).mean()
        
        # 결측값 처리 (특성 추출 후)
        data = data.fillna(0)
        return data

class HipotReferenceModel(nn.Module):
    """Hipot 기준 모델 (LSTM + Autoencoder + Classifier)"""
    
    def __init__(self, input_dim=4, hidden_dim=128, num_layers=3, latent_dim=16):
        super(HipotReferenceModel, self).__init__()
        
        self.input_dim = input_dim
        self.hidden_dim = hidden_dim
        self.num_layers = num_layers
        self.latent_dim = latent_dim
        
        # LSTM for temporal patterns
        self.lstm = nn.LSTM(input_dim, hidden_dim, num_layers, 
                           batch_first=True, dropout=0.2)
        
        # Autoencoder for anomaly detection
        self.encoder = nn.Sequential(
            nn.Linear(hidden_dim, 64),
            nn.ReLU(),
            nn.BatchNorm1d(64),
            nn.Linear(64, 32),
            nn.ReLU(),
            nn.BatchNorm1d(32),
            nn.Linear(32, latent_dim)
        )
        
        self.decoder = nn.Sequential(
            nn.Linear(latent_dim, 32),
            nn.ReLU(),
            nn.BatchNorm1d(32),
            nn.Linear(32, 64),
            nn.ReLU(),
            nn.BatchNorm1d(64),
            nn.Linear(64, hidden_dim)
        )
        
        # Classification head
        self.classifier = nn.Sequential(
            nn.Linear(latent_dim, 32),
            nn.ReLU(),
            nn.Dropout(0.3),
            nn.Linear(32, 16),
            nn.ReLU(),
            nn.Dropout(0.3),
            nn.Linear(16, 5)  # Valid, Error, OutOfRange, Critical, Dead
        )
        
    def forward(self, x):
        # LSTM forward pass
        lstm_out, _ = self.lstm(x)
        # 마지막 시퀀스의 출력 사용
        lstm_last = lstm_out[:, -1, :]
        
        # Autoencoder forward pass
        encoded = self.encoder(lstm_last)
        decoded = self.decoder(encoded)
        
        # Classification forward pass
        classified = self.classifier(encoded)
        
        return {
            'encoded': encoded,
            'decoded': decoded,
            'classified': classified,
            'lstm_output': lstm_last
        }

class AccuracyDefectCalculator:
    """정확도 및 불합률 계산 클래스"""
    
    def __init__(self, reference_model: HipotReferenceModel):
        self.reference_model = reference_model
        self.threshold_config = {
            'reconstruction_threshold': 0.1,
            'classification_confidence': 0.8,
            'temporal_deviation': 0.05,
            'pattern_similarity_threshold': 0.7
        }
        self.reference_patterns = None
        
    def set_reference_patterns(self, reference_data: Dict):
        """기준 패턴 설정"""
        self.reference_patterns = reference_data
        
    def calculate_accuracy(self, test_data: pd.DataFrame) -> Dict:
        """테스트 데이터의 정확도 계산"""
        
        if self.reference_patterns is None:
            raise ValueError("기준 패턴이 설정되지 않았습니다.")
        
        # 1. 패턴 유사도 계산
        pattern_similarity = self._calculate_pattern_similarity(test_data)
        
        # 2. 통계적 일치도 계산
        statistical_match = self._calculate_statistical_match(test_data)
        
        # 3. 시간적 일관성 평가
        temporal_consistency = self._evaluate_temporal_consistency(test_data)
        
        # 4. 종합 정확도 점수
        overall_accuracy = self._weighted_average([
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
    
    def calculate_defect_rate(self, test_results: List[Dict]) -> Dict:
        """테스트 결과의 불합률 계산"""
        
        if not test_results:
            return {
                'overall_defect_rate': 0.0,
                'defect_breakdown': {},
                'total_tests': 0,
                'pass_rate': 0.0
            }
        
        # 분류 수행
        classifications = []
        for result in test_results:
            classification = self._classify_test_result(result)
            classifications.append(classification)
        
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
    
    def _calculate_pattern_similarity(self, test_data: pd.DataFrame) -> float:
        """패턴 유사도 계산"""
        try:
            # 기준 패턴과의 코사인 유사도 계산
            test_features = test_data[['voltage', 'current', 'resistance']].values
            ref_features = self.reference_patterns['features']
            
            # 길이 맞추기 (짧은 쪽에 맞춤)
            min_len = min(len(test_features), len(ref_features))
            test_features = test_features[:min_len]
            ref_features = ref_features[:min_len]
            
            # 코사인 유사도 계산
            test_norm = np.linalg.norm(test_features)
            ref_norm = np.linalg.norm(ref_features)
            
            if test_norm == 0 or ref_norm == 0:
                return 0.0
                
            similarity = np.dot(test_features.flatten(), ref_features.flatten()) / (test_norm * ref_norm)
            return max(0.0, min(1.0, similarity))
            
        except Exception:
            return 0.5  # 기본값
    
    def _calculate_statistical_match(self, test_data: pd.DataFrame) -> float:
        """통계적 일치도 계산"""
        try:
            test_stats = {
                'voltage_mean': test_data['voltage'].mean(),
                'voltage_std': test_data['voltage'].std(),
                'current_mean': test_data['current'].mean(),
                'current_std': test_data['current'].std(),
                'resistance_mean': test_data['resistance'].mean(),
                'resistance_std': test_data['resistance'].std()
            }
            
            ref_stats = self.reference_patterns['statistics']
            
            # 각 통계량의 상대적 차이 계산
            matches = []
            for key in test_stats:
                if key in ref_stats and ref_stats[key] != 0:
                    diff = abs(test_stats[key] - ref_stats[key]) / abs(ref_stats[key])
                    match = max(0.0, 1.0 - diff)
                    matches.append(match)
            
            return np.mean(matches) if matches else 0.5
            
        except Exception:
            return 0.5
    
    def _evaluate_temporal_consistency(self, test_data: pd.DataFrame) -> float:
        """시간적 일관성 평가"""
        try:
            # 시간적 변화율의 일관성 확인
            voltage_diff = test_data['voltage'].diff().dropna()
            current_diff = test_data['current'].diff().dropna()
            
            # 급격한 변화의 비율 계산
            voltage_stability = 1.0 - (voltage_diff.abs() > voltage_diff.std() * 3).mean()
            current_stability = 1.0 - (current_diff.abs() > current_diff.std() * 3).mean()
            
            return (voltage_stability + current_stability) / 2
            
        except Exception:
            return 0.5
    
    def _classify_test_result(self, result: Dict) -> DataClassification:
        """개별 테스트 결과 분류"""
        # 기본적인 분류 로직 (실제로는 더 복잡한 규칙 필요)
        
        voltage = result.get('voltage', 0)
        current = result.get('current', 0)
        resistance = result.get('resistance', 0)
        
        # Dead 상태 확인
        if voltage == 0 and current == 0:
            return DataClassification.DEAD
        
        # Critical 상태 확인 (과전류)
        if current > 0.01:  # 10mA 초과
            return DataClassification.CRITICAL
        
        # Out of Range 확인
        if resistance < 1e3 or resistance > 1e12:
            return DataClassification.OUT_OF_RANGE
        
        # Error 상태 확인 (비정상적인 값)
        if voltage < 0 or current < 0 or resistance < 0:
            return DataClassification.ERROR
        
        # 기본적으로 Valid
        return DataClassification.VALID
    
    def _weighted_average(self, values_weights: List[Tuple[float, float]]) -> float:
        """가중 평균 계산"""
        total_value = sum(value * weight for value, weight in values_weights)
        total_weight = sum(weight for _, weight in values_weights)
        return total_value / total_weight if total_weight > 0 else 0.0

class HipotGraphGenerator:
    """Hipot 그래프 생성 클래스"""
    
    def __init__(self):
        plt.style.use('seaborn-v0_8')
        self.plot_configs = {
            'voltage_current': {'figsize': (12, 8), 'subplots': (2, 1)},
            'resistance_time': {'figsize': (10, 6)},
            'vi_characteristic': {'figsize': (8, 6)},
            'anomaly_heatmap': {'figsize': (10, 6)},
            'statistical_distribution': {'figsize': (15, 10), 'subplots': (2, 3)}
        }
    
    def create_comparison_plots(self, test_data: pd.DataFrame, reference_data: Dict) -> List[str]:
        """비교 그래프 생성"""
        
        plot_paths = []
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        
        # 1. 실시간 측정 그래프
        path1 = self._create_real_time_comparison(test_data, reference_data, timestamp)
        plot_paths.append(path1)
        
        # 2. 통계적 분포 비교
        path2 = self._create_statistical_analysis_plots(test_data, reference_data, timestamp)
        plot_paths.append(path2)
        
        # 3. 이상 탐지 히트맵
        path3 = self._create_anomaly_heatmap(test_data, timestamp)
        plot_paths.append(path3)
        
        return plot_paths
    
    def _create_real_time_comparison(self, test_data: pd.DataFrame, reference_data: Dict, timestamp: str) -> str:
        """실시간 비교 그래프 생성"""
        
        fig, axes = plt.subplots(2, 2, figsize=(15, 10))
        fig.suptitle(f'Hipot Test Data Comparison - {timestamp}', fontsize=16)
        
        time_test = test_data['time'] if 'time' in test_data.columns else range(len(test_data))
        
        # 전압-시간 비교
        axes[0, 0].plot(time_test, test_data['voltage'], 
                       label='Current Test', color='blue', alpha=0.7, linewidth=2)
        if 'voltage_ref' in reference_data:
            time_ref = range(len(reference_data['voltage_ref']))
            axes[0, 0].plot(time_ref, reference_data['voltage_ref'], 
                           label='Reference', color='red', linestyle='--', linewidth=2)
        axes[0, 0].set_title('Voltage vs Time Comparison')
        axes[0, 0].set_xlabel('Time (s)')
        axes[0, 0].set_ylabel('Voltage (V)')
        axes[0, 0].legend()
        axes[0, 0].grid(True, alpha=0.3)
        
        # 전류-시간 비교
        axes[0, 1].plot(time_test, test_data['current'], 
                       label='Current Test', color='blue', alpha=0.7, linewidth=2)
        if 'current_ref' in reference_data:
            axes[0, 1].plot(time_ref, reference_data['current_ref'], 
                           label='Reference', color='red', linestyle='--', linewidth=2)
        axes[0, 1].set_title('Current vs Time Comparison')
        axes[0, 1].set_xlabel('Time (s)')
        axes[0, 1].set_ylabel('Current (A)')
        axes[0, 1].legend()
        axes[0, 1].grid(True, alpha=0.3)
        
        # 저항-시간 비교 (로그 스케일)
        axes[1, 0].semilogy(time_test, test_data['resistance'], 
                           label='Current Test', color='blue', alpha=0.7, linewidth=2)
        if 'resistance_ref' in reference_data:
            axes[1, 0].semilogy(time_ref, reference_data['resistance_ref'], 
                               label='Reference', color='red', linestyle='--', linewidth=2)
        axes[1, 0].set_title('Resistance vs Time Comparison (Log Scale)')
        axes[1, 0].set_xlabel('Time (s)')
        axes[1, 0].set_ylabel('Resistance (Ω)')
        axes[1, 0].legend()
        axes[1, 0].grid(True, alpha=0.3)
        
        # V-I 특성 곡선
        axes[1, 1].plot(test_data['voltage'], test_data['current'], 
                       'bo-', alpha=0.7, markersize=3, label='Test Data')
        axes[1, 1].set_title('V-I Characteristic Curve')
        axes[1, 1].set_xlabel('Voltage (V)')
        axes[1, 1].set_ylabel('Current (A)')
        axes[1, 1].legend()
        axes[1, 1].grid(True, alpha=0.3)
        
        plt.tight_layout()
        
        filename = f'hipot_comparison_{timestamp}.png'
        plt.savefig(filename, dpi=300, bbox_inches='tight')
        plt.close()
        
        return filename
    
    def _create_statistical_analysis_plots(self, test_data: pd.DataFrame, reference_data: Dict, timestamp: str) -> str:
        """통계적 분석 그래프 생성"""
        
        fig, axes = plt.subplots(2, 3, figsize=(18, 12))
        fig.suptitle(f'Statistical Analysis - {timestamp}', fontsize=16)
        
        parameters = ['voltage', 'current', 'resistance']
        
        # 히스토그램 비교 (상단)
        for i, param in enumerate(parameters):
            axes[0, i].hist(test_data[param], bins=30, alpha=0.7, 
                           label='Test Data', color='blue', density=True)
            if f'{param}_ref' in reference_data:
                axes[0, i].hist(reference_data[f'{param}_ref'], bins=30, alpha=0.7, 
                               label='Reference Data', color='red', density=True)
            axes[0, i].set_title(f'{param.capitalize()} Distribution')
            axes[0, i].set_xlabel(param.capitalize())
            axes[0, i].set_ylabel('Density')
            axes[0, i].legend()
            axes[0, i].grid(True, alpha=0.3)
        
        # 박스 플롯 (하단)
        for i, param in enumerate(parameters):
            data_to_plot = [test_data[param]]
            labels = ['Test Data']
            
            if f'{param}_ref' in reference_data:
                data_to_plot.append(reference_data[f'{param}_ref'])
                labels.append('Reference Data')
            
            axes[1, i].boxplot(data_to_plot, labels=labels)
            axes[1, i].set_title(f'{param.capitalize()} Box Plot')
            axes[1, i].set_ylabel(param.capitalize())
            axes[1, i].grid(True, alpha=0.3)
        
        plt.tight_layout()
        
        filename = f'hipot_statistics_{timestamp}.png'
        plt.savefig(filename, dpi=300, bbox_inches='tight')
        plt.close()
        
        return filename
    
    def _create_anomaly_heatmap(self, test_data: pd.DataFrame, timestamp: str) -> str:
        """이상 탐지 히트맵 생성"""
        
        fig, axes = plt.subplots(1, 2, figsize=(15, 6))
        fig.suptitle(f'Anomaly Detection Heatmap - {timestamp}', fontsize=16)
        
        # 시간에 따른 파라미터 변화 히트맵
        params = ['voltage', 'current', 'resistance']
        if all(param in test_data.columns for param in params):
            # 데이터 정규화
            normalized_data = test_data[params].apply(lambda x: (x - x.min()) / (x.max() - x.min() + 1e-8))
            
            # 시간 윈도우별 히트맵
            window_size = max(1, len(normalized_data) // 20)
            windowed_data = []
            
            for i in range(0, len(normalized_data), window_size):
                window = normalized_data.iloc[i:i+window_size]
                windowed_data.append(window.mean().values)
            
            if windowed_data:
                heatmap_data = np.array(windowed_data).T
                
                im1 = axes[0].imshow(heatmap_data, aspect='auto', cmap='RdYlBu_r')
                axes[0].set_title('Parameter Evolution Heatmap')
                axes[0].set_xlabel('Time Windows')
                axes[0].set_ylabel('Parameters')
                axes[0].set_yticks(range(len(params)))
                axes[0].set_yticklabels(params)
                plt.colorbar(im1, ax=axes[0])
        
        # 상관관계 히트맵
        if len(test_data) > 1:
            corr_matrix = test_data[params].corr()
            im2 = axes[1].imshow(corr_matrix, aspect='auto', cmap='coolwarm', vmin=-1, vmax=1)
            axes[1].set_title('Parameter Correlation Heatmap')
            axes[1].set_xticks(range(len(params)))
            axes[1].set_yticks(range(len(params)))
            axes[1].set_xticklabels(params)
            axes[1].set_yticklabels(params)
            
            # 상관계수 값 표시
            for i in range(len(params)):
                for j in range(len(params)):
                    axes[1].text(j, i, f'{corr_matrix.iloc[i, j]:.2f}', 
                               ha='center', va='center', color='black')
            
            plt.colorbar(im2, ax=axes[1])
        
        plt.tight_layout()
        
        filename = f'hipot_anomaly_{timestamp}.png'
        plt.savefig(filename, dpi=300, bbox_inches='tight')
        plt.close()
        
        return filename

class HipotAIAnalyzer:
    """메인 Hipot AI 분석기 클래스"""
    
    def __init__(self, config_path: Optional[str] = None):
        self.config = self._load_config(config_path)
        self.preprocessor = HipotDataPreprocessor()
        self.reference_model = None
        self.graph_generator = HipotGraphGenerator()
        self.accuracy_calculator = None
        self.reference_data = None
        
        # 기본 설정
        self.device = torch.device('cuda' if torch.cuda.is_available() else 'cpu')
        
    def _load_config(self, config_path: Optional[str]) -> Dict:
        """설정 파일 로드"""
        default_config = {
            'model': {
                'input_dim': 4,
                'hidden_dim': 128,
                'num_layers': 3,
                'latent_dim': 16
            },
            'training': {
                'learning_rate': 0.001,
                'batch_size': 32,
                'epochs': 100,
                'early_stopping_patience': 10
            },
            'thresholds': {
                'reconstruction_threshold': 0.1,
                'classification_confidence': 0.8,
                'temporal_deviation': 0.05
            }
        }
        
        if config_path and os.path.exists(config_path):
            with open(config_path, 'r') as f:
                user_config = json.load(f)
                default_config.update(user_config)
        
        return default_config
    
    def initialize_model(self):
        """모델 초기화"""
        model_config = self.config['model']
        self.reference_model = HipotReferenceModel(
            input_dim=model_config['input_dim'],
            hidden_dim=model_config['hidden_dim'],
            num_layers=model_config['num_layers'],
            latent_dim=model_config['latent_dim']
        ).to(self.device)
        
        self.accuracy_calculator = AccuracyDefectCalculator(self.reference_model)
    
    def train_reference_model(self, training_data: List[pd.DataFrame]) -> Dict:
        """기준 모델 훈련"""
        if self.reference_model is None:
            self.initialize_model()
        
        # 훈련 데이터 전처리
        processed_data = []
        for data in training_data:
            processed = self.preprocessor.preprocess(data)
            processed_data.append(processed)
        
        # 훈련 데이터셋 생성
        train_dataset = self._create_dataset(processed_data)
        train_loader = DataLoader(train_dataset, 
                                batch_size=self.config['training']['batch_size'], 
                                shuffle=True)
        
        # 모델 훈련
        training_results = self._train_model(train_loader)
        
        # 기준 데이터 설정
        self._set_reference_data(processed_data)
        
        return training_results
    
    def analyze_test_session(self, test_data: pd.DataFrame) -> Dict:
        """완전한 테스트 세션 분석"""
        
        if self.reference_model is None or self.accuracy_calculator is None:
            raise ValueError("모델이 초기화되지 않았습니다. train_reference_model을 먼저 실행하세요.")
        
        # 1. 데이터 전처리
        processed_data = self.preprocessor.preprocess(test_data)
        
        # 2. 기준 모델과 비교
        accuracy_metrics = self.accuracy_calculator.calculate_accuracy(processed_data)
        
        # 3. 불합률 계산
        test_results = processed_data.to_dict('records')
        defect_metrics = self.accuracy_calculator.calculate_defect_rate(test_results)
        
        # 4. 그래프 생성
        comparison_plots = self.graph_generator.create_comparison_plots(
            processed_data, self.reference_data
        )
        
        # 5. 결과 리포트 생성
        report = self._generate_analysis_report(
            accuracy_metrics, defect_metrics, comparison_plots, processed_data
        )
        
        return report
    
    def _create_dataset(self, data_list: List[pd.DataFrame]) -> TensorDataset:
        """PyTorch 데이터셋 생성"""
        sequences = []
        
        for data in data_list:
            # 정규화된 특성 사용
            features = ['time_norm', 'voltage_norm', 'current_norm', 'resistance_norm']
            if all(col in data.columns for col in features):
                seq = data[features].values.astype(np.float32)
                sequences.append(seq)
        
        if not sequences:
            # 기본 특성 사용
            for data in data_list:
                seq = data[['time', 'voltage', 'current', 'resistance']].values.astype(np.float32)
                sequences.append(seq)
        
        # 시퀀스 길이 맞추기 (패딩 또는 트리밍)
        max_len = max(len(seq) for seq in sequences)
        min_len = min(len(seq) for seq in sequences)
        target_len = min(max_len, 1000)  # 최대 1000 포인트로 제한
        
        padded_sequences = []
        for seq in sequences:
            if len(seq) > target_len:
                # 균등하게 샘플링
                indices = np.linspace(0, len(seq)-1, target_len).astype(int)
                seq = seq[indices]
            elif len(seq) < target_len:
                # 마지막 값으로 패딩
                padding = np.tile(seq[-1:], (target_len - len(seq), 1))
                seq = np.vstack([seq, padding])
            
            padded_sequences.append(seq)
        
        sequences_tensor = torch.FloatTensor(np.array(padded_sequences))
        return TensorDataset(sequences_tensor, sequences_tensor)  # Autoencoder용
    
    def _train_model(self, train_loader: DataLoader) -> Dict:
        """모델 훈련 실행"""
        self.reference_model.train()
        
        optimizer = optim.Adam(self.reference_model.parameters(), 
                              lr=self.config['training']['learning_rate'])
        
        reconstruction_criterion = nn.MSELoss()
        classification_criterion = nn.CrossEntropyLoss()
        
        train_losses = []
        best_loss = float('inf')
        patience_counter = 0
        
        for epoch in range(self.config['training']['epochs']):
            epoch_loss = 0.0
            
            for batch_idx, (data, target) in enumerate(train_loader):
                data = data.to(self.device)
                target = target.to(self.device)
                
                optimizer.zero_grad()
                
                # Forward pass
                outputs = self.reference_model(data)
                
                # Loss 계산
                reconstruction_loss = reconstruction_criterion(outputs['decoded'], outputs['lstm_output'])
                
                # 가상의 분류 타겟 생성 (실제로는 라벨링된 데이터 필요)
                batch_size = data.size(0)
                fake_labels = torch.zeros(batch_size, dtype=torch.long).to(self.device)  # 모두 'VALID'로 가정
                classification_loss = classification_criterion(outputs['classified'], fake_labels)
                
                total_loss = reconstruction_loss + 0.1 * classification_loss
                
                total_loss.backward()
                optimizer.step()
                
                epoch_loss += total_loss.item()
            
            avg_loss = epoch_loss / len(train_loader)
            train_losses.append(avg_loss)
            
            # Early stopping
            if avg_loss < best_loss:
                best_loss = avg_loss
                patience_counter = 0
                # 모델 저장
                torch.save(self.reference_model.state_dict(), 'best_model.pth')
            else:
                patience_counter += 1
                if patience_counter >= self.config['training']['early_stopping_patience']:
                    print(f"Early stopping at epoch {epoch}")
                    break
            
            if epoch % 10 == 0:
                print(f"Epoch {epoch}, Loss: {avg_loss:.6f}")
        
        # 최적 모델 로드
        self.reference_model.load_state_dict(torch.load('best_model.pth'))
        self.reference_model.eval()
        
        return {
            'final_loss': best_loss,
            'training_losses': train_losses,
            'epochs_trained': len(train_losses)
        }
    
    def _set_reference_data(self, processed_data: List[pd.DataFrame]):
        """기준 데이터 설정"""
        if not processed_data:
            return
        
        # 첫 번째 데이터를 기준으로 사용 (실제로는 더 정교한 로직 필요)
        ref_data = processed_data[0]
        
        self.reference_data = {
            'features': ref_data[['voltage', 'current', 'resistance']].values,
            'voltage_ref': ref_data['voltage'].values,
            'current_ref': ref_data['current'].values,
            'resistance_ref': ref_data['resistance'].values,
            'statistics': {
                'voltage_mean': ref_data['voltage'].mean(),
                'voltage_std': ref_data['voltage'].std(),
                'current_mean': ref_data['current'].mean(),
                'current_std': ref_data['current'].std(),
                'resistance_mean': ref_data['resistance'].mean(),
                'resistance_std': ref_data['resistance'].std()
            }
        }
        
        # 정확도 계산기에 기준 패턴 설정
        self.accuracy_calculator.set_reference_patterns(self.reference_data)
    
    def _generate_analysis_report(self, accuracy_metrics: Dict, defect_metrics: Dict, 
                                plot_paths: List[str], processed_data: pd.DataFrame) -> Dict:
        """분석 리포트 생성"""
        
        return {
            'timestamp': datetime.now().isoformat(),
            'test_summary': {
                'data_points': len(processed_data),
                'test_duration': processed_data['time'].max() - processed_data['time'].min() if 'time' in processed_data.columns else 0,
                'voltage_range': [processed_data['voltage'].min(), processed_data['voltage'].max()],
                'current_range': [processed_data['current'].min(), processed_data['current'].max()],
                'resistance_range': [processed_data['resistance'].min(), processed_data['resistance'].max()]
            },
            'accuracy_metrics': accuracy_metrics,
            'defect_metrics': defect_metrics,
            'plots': plot_paths,
            'recommendations': self._generate_recommendations(accuracy_metrics, defect_metrics)
        }
    
    def _generate_recommendations(self, accuracy_metrics: Dict, defect_metrics: Dict) -> List[str]:
        """권장사항 생성"""
        recommendations = []
        
        if accuracy_metrics['overall_accuracy'] < 0.7:
            recommendations.append("전체 정확도가 낮습니다. 테스트 조건을 재검토하세요.")
        
        if defect_metrics['overall_defect_rate'] > 10:
            recommendations.append("불합률이 높습니다. 제품 품질을 점검하세요.")
        
        if accuracy_metrics['temporal_consistency'] < 0.6:
            recommendations.append("시간적 일관성이 부족합니다. 측정 환경의 안정성을 확인하세요.")
        
        if defect_metrics['defect_breakdown'].get('critical', 0) > 5:
            recommendations.append("치명적 결함이 감지되었습니다. 즉시 생산을 중단하고 원인을 파악하세요.")
        
        if not recommendations:
            recommendations.append("테스트 결과가 양호합니다.")
        
        return recommendations
    
    def save_model(self, filepath: str):
        """모델 저장"""
        if self.reference_model is not None:
            torch.save({
                'model_state_dict': self.reference_model.state_dict(),
                'config': self.config,
                'reference_data': self.reference_data
            }, filepath)
    
    def load_model(self, filepath: str):
        """모델 로드"""
        checkpoint = torch.load(filepath, map_location=self.device)
        
        self.config.update(checkpoint['config'])
        self.initialize_model()
        self.reference_model.load_state_dict(checkpoint['model_state_dict'])
        self.reference_data = checkpoint['reference_data']
        
        if self.reference_data:
            self.accuracy_calculator.set_reference_patterns(self.reference_data)

# 사용 예시 및 테스트 함수
def create_sample_data() -> pd.DataFrame:
    """샘플 데이터 생성 (테스트용)"""
    np.random.seed(42)
    
    time = np.linspace(0, 10, 100)
    voltage = 1000 + 500 * np.sin(0.5 * time) + np.random.normal(0, 10, 100)
    current = 0.001 + 0.0005 * np.sin(0.5 * time + np.pi/4) + np.random.normal(0, 0.0001, 100)
    resistance = voltage / (current + 1e-10) + np.random.normal(0, 1e6, 100)
    
    return pd.DataFrame({
        'time': time,
        'voltage': voltage,
        'current': current,
        'resistance': resistance
    })

if __name__ == "__main__":
    # 테스트 실행
    import os
    
    print("Hipot AI Analyzer 테스트 시작...")
    
    # 분석기 초기화
    analyzer = HipotAIAnalyzer()
    
    # 샘플 데이터 생성
    sample_data = [create_sample_data() for _ in range(5)]
    
    # 모델 훈련
    print("기준 모델 훈련 중...")
    training_results = analyzer.train_reference_model(sample_data)
    print(f"훈련 완료. 최종 손실: {training_results['final_loss']:.6f}")
    
    # 테스트 데이터 분석
    print("테스트 데이터 분석 중...")
    test_data = create_sample_data()
    analysis_result = analyzer.analyze_test_session(test_data)
    
    # 결과 출력
    print("\n=== 분석 결과 ===")
    print(f"전체 정확도: {analysis_result['accuracy_metrics']['overall_accuracy']:.3f}")
    print(f"불합률: {analysis_result['defect_metrics']['overall_defect_rate']:.1f}%")
    print(f"합격률: {analysis_result['defect_metrics']['pass_rate']:.1f}%")
    
    print("\n권장사항:")
    for recommendation in analysis_result['recommendations']:
        print(f"- {recommendation}")
    
    print(f"\n생성된 그래프: {len(analysis_result['plots'])}개")
    for plot_path in analysis_result['plots']:
        print(f"- {plot_path}")
    
    # 모델 저장
    analyzer.save_model('hipot_reference_model.pth')
    print("\n모델이 저장되었습니다: hipot_reference_model.pth")
    
    print("\nHipot AI Analyzer 테스트 완료!")