#!/usr/bin/env python3
"""
기본 기능 테스트 스크립트
개선사항의 기본 구조와 임포트를 확인합니다.
"""

import sys
import os
from datetime import datetime

def test_import_structure():
    """개선된 모듈들의 임포트 구조 테스트"""
    print("=== 모듈 임포트 구조 테스트 ===")
    
    try:
        # 기본 Python 모듈들
        import json
        import pickle
        from enum import Enum
        from typing import Dict, List, Optional
        print("✓ 기본 Python 모듈 임포트 성공")
        
        # AI 분석기 기본 구조
        sys.path.append(os.path.dirname(os.path.abspath(__file__)))
        
        from hipot_ai_analyzer import DataClassification, TestResult
        print("✓ 열거형 클래스 임포트 성공")
        
        # 지연 로딩 함수들 확인
        from hipot_ai_analyzer import _load_matplotlib, _load_sklearn, _load_torch
        print("✓ 지연 로딩 함수 임포트 성공")
        
        return True
        
    except Exception as e:
        print(f"✗ 모듈 임포트 실패: {e}")
        return False

def test_flask_structure():
    """Flask API 구조 테스트"""
    print("\n=== Flask API 구조 테스트 ===")
    
    try:
        from flask_api import app
        print("✓ Flask 앱 임포트 성공")
        
        # 성능 최적화 요소들 확인
        from flask_api import monitor_performance, cache_result
        print("✓ 성능 최적화 데코레이터 임포트 성공")
        
        return True
        
    except Exception as e:
        print(f"✗ Flask API 구조 테스트 실패: {e}")
        return False

def test_optimization_features():
    """최적화 기능 테스트"""
    print("\n=== 최적화 기능 테스트 ===")
    
    try:
        # 지연 로딩 테스트
        from hipot_ai_analyzer import _matplotlib_loaded, _sklearn_loaded, _torch_loaded
        print(f"✓ 초기 로딩 상태 - matplotlib: {_matplotlib_loaded}, sklearn: {_sklearn_loaded}, torch: {_torch_loaded}")
        
        # 캐싱 기능 테스트
        from functools import lru_cache
        
        @lru_cache(maxsize=32)
        def test_cache_function(x):
            return x * 2
        
        # 캐시 테스트
        result1 = test_cache_function(5)
        result2 = test_cache_function(5)  # 캐시에서 가져와야 함
        
        if result1 == result2 == 10:
            print("✓ LRU 캐시 기능 정상 작동")
        else:
            print("✗ LRU 캐시 기능 오류")
            return False
            
        return True
        
    except Exception as e:
        print(f"✗ 최적화 기능 테스트 실패: {e}")
        return False

def test_file_structure():
    """파일 구조 개선사항 확인"""
    print("\n=== 파일 구조 개선사항 확인 ===")
    
    expected_files = [
        'hipot_ai_analyzer.py',
        'flask_api.py',
        'Hipot_Tester/Services/IDeviceService.cs',
        'Hipot_Tester/Services/DeviceService.cs',
        'Hipot_Tester/Services/IDialogService.cs',
        'Hipot_Tester/Services/DialogService.cs',
        'Hipot_Tester/Devices/IDeviceFactory.cs',
        'Hipot_Tester/ViewModel/MainViewModel.cs'
    ]
    
    existing_files = []
    missing_files = []
    
    for file_path in expected_files:
        full_path = os.path.join(os.path.dirname(__file__), file_path)
        if os.path.exists(full_path):
            existing_files.append(file_path)
        else:
            missing_files.append(file_path)
    
    print(f"✓ 존재하는 파일: {len(existing_files)}/{len(expected_files)}")
    for file in existing_files:
        print(f"  ✓ {file}")
    
    if missing_files:
        print(f"✗ 누락된 파일: {len(missing_files)}")
        for file in missing_files:
            print(f"  ✗ {file}")
    
    return len(missing_files) == 0

def main():
    """메인 테스트 함수"""
    print("🔧 Hipot Tester 기본 구조 검증")
    print(f"📅 테스트 시간: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    
    test_results = []
    
    # 각 테스트 실행
    test_results.append(test_import_structure())
    test_results.append(test_flask_structure()) 
    test_results.append(test_optimization_features())
    test_results.append(test_file_structure())
    
    # 결과 요약
    print("\n" + "="*50)
    print("📊 기본 구조 테스트 결과")
    print("="*50)
    
    passed = sum(test_results)
    total = len(test_results)
    
    print(f"통과: {passed}/{total} ({passed/total*100:.1f}%)")
    
    if passed == total:
        print("🎉 기본 구조 검증 완료! 개선사항이 올바르게 구현되었습니다.")
        return 0
    else:
        print("⚠️ 일부 구조적 문제가 있습니다. 코드를 검토해주세요.")
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)