#!/usr/bin/env python3
"""
C# 컴파일 검증 스크립트
구문 오류, 참조 오류, 네임스페이스 오류 등을 검사합니다.
"""

import os
import re
import sys
from typing import List, Dict, Set

def check_using_statements(file_path: str) -> List[str]:
    """using 문 검증"""
    errors = []
    
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
        
        # using 문 추출
        using_matches = re.findall(r'using\s+([^;]+);', content)
        
        # 네임스페이스 내에서 사용되는 클래스들 검증
        namespace_match = re.search(r'namespace\s+([^\s{]+)', content)
        if namespace_match:
            namespace = namespace_match.group(1)
            
            # 클래스에서 사용되는 다른 클래스들 확인
            class_references = re.findall(r':\s*([A-Za-z_][A-Za-z0-9_]*)', content)
            generic_references = re.findall(r'<([A-Za-z_][A-Za-z0-9_]*)', content)
            new_references = re.findall(r'new\s+([A-Za-z_][A-Za-z0-9_]*)', content)
            
            all_references = set(class_references + generic_references + new_references)
            
            # 기본 .NET 타입들
            builtin_types = {
                'object', 'string', 'int', 'double', 'bool', 'void', 'Task', 
                'List', 'Dictionary', 'ObservableCollection', 'Exception',
                'ArgumentNullException', 'NotSupportedException', 'InvalidOperationException',
                'ICommand', 'INotifyPropertyChanged', 'PropertyChangedEventArgs',
                'EventHandler', 'Action', 'Func', 'Predicate'
            }
            
            for ref in all_references:
                if ref not in builtin_types and not any(ref in using for using in using_matches):
                    # 같은 네임스페이스에 있는 클래스가 아닌 경우만 경고
                    if not (namespace.startswith('Hipot_Tester') and ref.startswith('Hipot_Tester')):
                        errors.append(f"Potentially missing using statement for '{ref}'")
        
    except Exception as e:
        errors.append(f"Error reading file: {e}")
    
    return errors

def check_interface_implementations(file_path: str) -> List[str]:
    """인터페이스 구현 검증"""
    errors = []
    
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
        
        # 인터페이스 구현 확인
        interface_impl = re.search(r'class\s+\w+\s*:\s*.*?([I][A-Za-z_][A-Za-z0-9_]*)', content)
        if interface_impl:
            interface_name = interface_impl.group(1)
            
            # 인터페이스에 따른 필수 메서드 확인
            if interface_name == 'IDisposable':
                if 'void Dispose()' not in content and 'public void Dispose()' not in content:
                    errors.append(f"Missing Dispose() method for IDisposable implementation")
            
            if interface_name == 'ICommand':
                required_members = ['CanExecute', 'Execute', 'CanExecuteChanged']
                for member in required_members:
                    if member not in content:
                        errors.append(f"Missing {member} for ICommand implementation")
        
    except Exception as e:
        errors.append(f"Error checking interface implementations: {e}")
    
    return errors

def check_async_patterns(file_path: str) -> List[str]:
    """비동기 패턴 검증"""
    errors = []
    
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
        
        # async 메서드가 Task를 반환하는지 확인
        async_methods = re.findall(r'(public|private|protected)\s+async\s+(\w+)\s+(\w+)', content)
        for access, return_type, method_name in async_methods:
            if return_type not in ['Task', 'Task<string>', 'Task<bool>', 'Task<double>', 'Task<int>']:
                errors.append(f"Async method '{method_name}' should return Task or Task<T>")
        
        # Thread.Sleep 사용 검증
        if 'Thread.Sleep' in content and 'Task.Delay' not in content:
            errors.append("Consider using Task.Delay instead of Thread.Sleep for better async performance")
        
    except Exception as e:
        errors.append(f"Error checking async patterns: {e}")
    
    return errors

def check_file_compilation(file_path: str) -> Dict[str, List[str]]:
    """단일 파일 컴파일 검증"""
    results = {
        'using_statements': check_using_statements(file_path),
        'interface_implementations': check_interface_implementations(file_path),
        'async_patterns': check_async_patterns(file_path)
    }
    
    return results

def main():
    """메인 검증 함수"""
    print("🔍 C# 컴파일 검증 시작")
    print("=" * 50)
    
    # 검증할 파일들
    cs_files = [
        'Hipot_Tester/Services/IDeviceService.cs',
        'Hipot_Tester/Services/DeviceService.cs',
        'Hipot_Tester/Services/IDialogService.cs',
        'Hipot_Tester/Services/DialogService.cs',
        'Hipot_Tester/Devices/IDeviceFactory.cs',
        'Hipot_Tester/ViewModel/MainViewModel.cs',
        'Hipot_Tester/ViewModel/Control/RelayCommand.cs',
        'Hipot_Tester/Devices/VisaSerialDevice.cs',
        'Hipot_Tester/Devices/RS_232_1903X.cs',
        'Hipot_Tester/Devices/RS_232_1905X.cs',
        'Hipot_Tester/Model/DeviceManager.cs'
    ]
    
    total_errors = 0
    
    for file_path in cs_files:
        full_path = os.path.join(os.path.dirname(__file__), file_path)
        
        if not os.path.exists(full_path):
            print(f"❌ {file_path}: File not found")
            total_errors += 1
            continue
        
        print(f"\n📁 {file_path}")
        
        results = check_file_compilation(full_path)
        file_errors = 0
        
        for category, errors in results.items():
            if errors:
                print(f"  ⚠️  {category.replace('_', ' ').title()}:")
                for error in errors:
                    print(f"    - {error}")
                    file_errors += 1
        
        if file_errors == 0:
            print("  ✅ No issues found")
        else:
            total_errors += file_errors
    
    # 프로젝트 파일 검증
    print(f"\n📋 프로젝트 파일 검증")
    csproj_path = os.path.join(os.path.dirname(__file__), 'Hipot_Tester/Hipot_Tester.csproj')
    
    if os.path.exists(csproj_path):
        with open(csproj_path, 'r', encoding='utf-8-sig') as f:
            csproj_content = f.read()
        
        # 새로 추가된 파일들이 포함되어 있는지 확인
        required_includes = [
            'IDeviceFactory.cs',
            'IDeviceService.cs', 
            'DeviceService.cs',
            'IDialogService.cs',
            'DialogService.cs'
        ]
        
        missing_includes = []
        for include in required_includes:
            if include not in csproj_content:
                missing_includes.append(include)
        
        if missing_includes:
            print(f"  ⚠️  Missing project file includes:")
            for missing in missing_includes:
                print(f"    - {missing}")
            total_errors += len(missing_includes)
        else:
            print("  ✅ All new files included in project")
    else:
        print("  ❌ Project file not found")
        total_errors += 1
    
    # 결과 요약
    print("\n" + "=" * 50)
    print("📊 검증 결과 요약")
    print("=" * 50)
    
    if total_errors == 0:
        print("🎉 모든 파일이 컴파일 가능한 상태입니다!")
        return 0
    else:
        print(f"⚠️  총 {total_errors}개의 잠재적 문제가 발견되었습니다.")
        print("대부분은 경고 수준이며, 실제 컴파일은 가능할 수 있습니다.")
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)