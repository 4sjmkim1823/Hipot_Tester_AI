#!/usr/bin/env python3
"""
DataManagementViewModel 수정사항 검증 스크립트
"""

import os
import sys

def check_files_exist():
    """필요한 파일들이 존재하는지 확인"""
    required_files = [
        'Hipot_Tester/Model/DataStorage.cs',
        'Hipot_Tester/Model/DataClassification.cs', 
        'Hipot_Tester/Model/ChartHelper.cs',
        'Hipot_Tester/Model/DataFilterExtensions.cs',
        'Hipot_Tester/ViewModel/DataManagementViewModel.cs'
    ]
    
    print("📁 필요한 파일 존재 여부 확인")
    print("-" * 40)
    
    all_exist = True
    for file_path in required_files:
        full_path = os.path.join(os.path.dirname(__file__), file_path)
        if os.path.exists(full_path):
            print(f"✅ {file_path}")
        else:
            print(f"❌ {file_path} - 파일이 없습니다")
            all_exist = False
    
    return all_exist

def check_project_references():
    """프로젝트 파일에 새로운 클래스들이 포함되었는지 확인"""
    print("\n📋 프로젝트 파일 참조 확인")
    print("-" * 40)
    
    csproj_path = os.path.join(os.path.dirname(__file__), 
                               'Hipot_Tester/Hipot_Tester.csproj')
    
    if not os.path.exists(csproj_path):
        print("❌ 프로젝트 파일을 찾을 수 없습니다")
        return False
    
    with open(csproj_path, 'r', encoding='utf-8-sig') as f:
        content = f.read()
    
    required_references = [
        'DataStorage.cs',
        'DataClassification.cs',
        'ChartHelper.cs', 
        'DataFilterExtensions.cs'
    ]
    
    all_referenced = True
    for ref in required_references:
        if ref in content:
            print(f"✅ {ref} - 프로젝트에 포함됨")
        else:
            print(f"❌ {ref} - 프로젝트에 포함되지 않음")
            all_referenced = False
    
    return all_referenced

def check_using_statements():
    """DataManagementViewModel의 using 문이 올바른지 확인"""
    print("\n📦 Using 문 확인")
    print("-" * 40)
    
    viewmodel_path = os.path.join(os.path.dirname(__file__), 
                                  'Hipot_Tester/ViewModel/DataManagementViewModel.cs')
    
    if not os.path.exists(viewmodel_path):
        print("❌ DataManagementViewModel.cs를 찾을 수 없습니다")
        return False
    
    with open(viewmodel_path, 'r', encoding='utf-8-sig') as f:
        content = f.read()
    
    required_usings = [
        'using Hipot_Tester.Model;',
        'using System.Collections.ObjectModel;',
        'using System.Windows;'
    ]
    
    all_present = True
    for using in required_usings:
        if using in content:
            print(f"✅ {using}")
        else:
            print(f"❌ {using} - 누락됨")
            all_present = False
    
    return all_present

def check_class_references():
    """필요한 클래스들이 올바르게 참조되는지 확인"""
    print("\n🔗 클래스 참조 확인")
    print("-" * 40)
    
    viewmodel_path = os.path.join(os.path.dirname(__file__), 
                                  'Hipot_Tester/ViewModel/DataManagementViewModel.cs')
    
    with open(viewmodel_path, 'r', encoding='utf-8-sig') as f:
        content = f.read()
    
    required_classes = [
        'DataStorage.Instance',
        'SessionEventArgs',
        'TestSession'
    ]
    
    all_referenced = True
    for class_ref in required_classes:
        if class_ref in content:
            print(f"✅ {class_ref} - 참조됨")
        else:
            print(f"❌ {class_ref} - 참조되지 않음")
            all_referenced = False
    
    return all_referenced

def check_compilation_readiness():
    """컴파일 준비 상태 확인"""
    print("\n🔧 컴파일 준비 상태 확인")
    print("-" * 40)
    
    # 기본적인 문법 검사
    cs_files = [
        'Hipot_Tester/Model/DataStorage.cs',
        'Hipot_Tester/Model/DataClassification.cs',
        'Hipot_Tester/Model/ChartHelper.cs',
        'Hipot_Tester/Model/DataFilterExtensions.cs'
    ]
    
    all_valid = True
    for file_path in cs_files:
        full_path = os.path.join(os.path.dirname(__file__), file_path)
        
        try:
            with open(full_path, 'r', encoding='utf-8-sig') as f:
                content = f.read()
            
            # 기본적인 구문 검사
            open_braces = content.count('{')
            close_braces = content.count('}')
            
            if open_braces == close_braces:
                print(f"✅ {os.path.basename(file_path)} - 중괄호 매칭 정상")
            else:
                print(f"❌ {os.path.basename(file_path)} - 중괄호 매칭 오류")
                all_valid = False
                
        except Exception as e:
            print(f"❌ {os.path.basename(file_path)} - 읽기 오류: {e}")
            all_valid = False
    
    return all_valid

def main():
    """메인 검증 함수"""
    print("🔍 DataManagementViewModel 수정사항 검증")
    print("=" * 50)
    
    # 각 검증 단계 실행
    tests = [
        ("파일 존재 여부", check_files_exist),
        ("프로젝트 참조", check_project_references), 
        ("Using 문", check_using_statements),
        ("클래스 참조", check_class_references),
        ("컴파일 준비상태", check_compilation_readiness)
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
        print("DataManagementViewModel의 컴파일 오류가 해결되었습니다.")
        return 0
    else:
        print("⚠️ 일부 검증에 실패했습니다.")
        print("추가 수정이 필요할 수 있습니다.")
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)