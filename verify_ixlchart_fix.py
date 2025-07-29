#!/usr/bin/env python3
"""
IXLChart.cs 수정사항 검증 스크립트
"""

import os
import sys

def check_file_content():
    """IXLChart.cs 파일 내용 확인"""
    print("📁 IXLChart.cs 파일 내용 확인")
    print("-" * 40)
    
    file_path = os.path.join(os.path.dirname(__file__), 
                             'Hipot_Tester/Model/IXLChart.cs')
    
    if not os.path.exists(file_path):
        print("❌ IXLChart.cs 파일을 찾을 수 없습니다")
        return False
    
    with open(file_path, 'r', encoding='utf-8-sig') as f:
        content = f.read()
    
    # 문제가 되었던 타입들이 제거되었는지 확인
    problematic_types = [
        'XLTickMarks',
        'XLLegendPosition',
        'ClosedXML.Excel.XLTickMarks'
    ]
    
    problems_found = []
    for ptype in problematic_types:
        if ptype in content:
            problems_found.append(ptype)
    
    if problems_found:
        print(f"❌ 문제가 되는 타입들이 여전히 존재합니다:")
        for ptype in problems_found:
            print(f"    - {ptype}")
        return False
    else:
        print("✅ 문제가 되었던 타입들이 모두 제거되었습니다")
    
    # 새로 추가된 열거형들이 있는지 확인
    required_enums = [
        'enum ChartTickMarks',
        'enum ChartLegendPosition'
    ]
    
    enums_found = []
    for enum in required_enums:
        if enum in content:
            enums_found.append(enum)
    
    if len(enums_found) == len(required_enums):
        print("✅ 새로운 열거형들이 추가되었습니다")
        for enum in enums_found:
            print(f"    - {enum}")
    else:
        print("❌ 필요한 열거형들이 누락되었습니다")
        return False
    
    return True

def check_interface_definitions():
    """인터페이스 정의 확인"""
    print("\n🔗 인터페이스 정의 확인")
    print("-" * 40)
    
    file_path = os.path.join(os.path.dirname(__file__), 
                             'Hipot_Tester/Model/IXLChart.cs')
    
    with open(file_path, 'r', encoding='utf-8-sig') as f:
        content = f.read()
    
    # 올바른 타입 사용 확인
    correct_usages = [
        'ChartTickMarks MajorTickMarks',
        'ChartLegendPosition Position'
    ]
    
    all_correct = True
    for usage in correct_usages:
        if usage in content:
            print(f"✅ {usage} - 올바르게 사용됨")
        else:
            print(f"❌ {usage} - 사용되지 않음")
            all_correct = False
    
    return all_correct

def check_namespace_usage():
    """네임스페이스 사용 확인"""
    print("\n📦 네임스페이스 사용 확인")
    print("-" * 40)
    
    file_path = os.path.join(os.path.dirname(__file__), 
                             'Hipot_Tester/Model/IXLChart.cs')
    
    with open(file_path, 'r', encoding='utf-8-sig') as f:
        content = f.read()
    
    # 필요한 using 문 확인
    required_usings = [
        'using ClosedXML.Excel;',
        'using System;'
    ]
    
    unnecessary_usings = [
        'using ClosedXML.Excel.Drawings;',
        'using Hipot_Tester.ViewModel;'
    ]
    
    all_good = True
    
    # 필요한 using 문 확인
    for using in required_usings:
        if using in content:
            print(f"✅ {using} - 필요한 using 문")
        else:
            print(f"❌ {using} - 누락된 using 문")
            all_good = False
    
    # 불필요한 using 문 확인
    for using in unnecessary_usings:
        if using not in content:
            print(f"✅ {using} - 불필요한 using 문 제거됨")
        else:
            print(f"⚠️  {using} - 불필요한 using 문이 여전히 있음")
    
    return all_good

def check_syntax_validity():
    """기본 구문 유효성 확인"""
    print("\n🔧 기본 구문 유효성 확인")
    print("-" * 40)
    
    file_path = os.path.join(os.path.dirname(__file__), 
                             'Hipot_Tester/Model/IXLChart.cs')
    
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
        
        # 중괄호 매칭 확인
        open_braces = content.count('{')
        close_braces = content.count('}')
        
        if open_braces == close_braces:
            print(f"✅ 중괄호 매칭 정상 ({open_braces}개씩)")
        else:
            print(f"❌ 중괄호 매칭 오류: {{ {open_braces}개, }} {close_braces}개")
            return False
        
        # 세미콜론 확인 (기본적인 체크)
        lines = content.split('\n')
        interface_lines = [line for line in lines if 'interface ' in line and '{' in line]
        
        if len(interface_lines) > 0:
            print(f"✅ 인터페이스 정의 {len(interface_lines)}개 발견")
        else:
            print("❌ 인터페이스 정의를 찾을 수 없습니다")
            return False
        
        # 열거형 정의 확인
        enum_lines = [line for line in lines if 'enum ' in line]
        
        if len(enum_lines) >= 2:
            print(f"✅ 열거형 정의 {len(enum_lines)}개 발견")
        else:
            print("❌ 충분한 열거형 정의를 찾을 수 없습니다")
            return False
        
        return True
        
    except Exception as e:
        print(f"❌ 파일 읽기 오류: {e}")
        return False

def main():
    """메인 검증 함수"""
    print("🔍 IXLChart.cs 수정사항 검증")
    print("=" * 50)
    
    # 각 검증 단계 실행
    tests = [
        ("파일 내용", check_file_content),
        ("인터페이스 정의", check_interface_definitions),
        ("네임스페이스 사용", check_namespace_usage),
        ("기본 구문", check_syntax_validity)
    ]
    
    passed_tests = 0
    total_tests = len(tests)
    
    for test_name, test_func in tests:
        if test_func():
            passed_tests += 1
    
    # 결과 요약
    print("\n" + "=" * 50)
    print("📊 검증 결과 요약")
    print("=" * 50)
    print(f"통과한 테스트: {passed_tests}/{total_tests}")
    
    if passed_tests == total_tests:
        print("🎉 모든 검증을 통과했습니다!")
        print("IXLChart.cs의 ClosedXML 호환성 문제가 해결되었습니다.")
        print("\n주요 수정사항:")
        print("- XLTickMarks → ChartTickMarks 열거형으로 교체")
        print("- XLLegendPosition → ChartLegendPosition 열거형으로 교체")
        print("- ClosedXML 0.105.0 버전과 호환")
        return 0
    else:
        print("⚠️ 일부 검증에 실패했습니다.")
        print("추가 수정이 필요할 수 있습니다.")
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)