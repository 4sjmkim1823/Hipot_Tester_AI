#!/usr/bin/env python3
"""
C# 기본 문법 검증 스크립트
"""

import os
import re
import sys
from typing import List

def check_basic_syntax(file_path: str) -> List[str]:
    """기본 문법 오류 검사"""
    errors = []
    
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as f:
            content = f.read()
        
        lines = content.split('\n')
        
        for i, line in enumerate(lines, 1):
            line = line.strip()
            
            # 중괄호 매칭 검사 (간단한 버전)
            open_braces = line.count('{')
            close_braces = line.count('}')
            
            # 세미콜론 누락 검사 (기본적인 경우만)
            if (line.endswith(')') and 
                not line.startswith('if') and 
                not line.startswith('for') and 
                not line.startswith('while') and 
                not line.startswith('using') and
                not line.endswith(';') and
                not line.endswith('{') and
                '{' not in line):
                errors.append(f"Line {i}: Missing semicolon")
            
            # 기본적인 구문 오류
            if 'public class' in line and not line.endswith('{') and '{' not in line:
                if i < len(lines) - 1 and not lines[i].strip().startswith('{'):
                    errors.append(f"Line {i}: Class declaration should be followed by opening brace")
        
        # 전체적인 중괄호 매칭 검사
        total_open = content.count('{')
        total_close = content.count('}')
        if total_open != total_close:
            errors.append(f"Mismatched braces: {total_open} opening, {total_close} closing")
        
        # 네임스페이스 검사
        if 'namespace ' in content and content.count('namespace ') > 1:
            errors.append("Multiple namespace declarations found")
        
    except Exception as e:
        errors.append(f"Error reading file: {e}")
    
    return errors

def main():
    """메인 검증 함수"""
    print("🔍 C# 기본 문법 검증")
    print("=" * 40)
    
    # 검증할 파일들
    cs_files = [
        'Hipot_Tester/Services/IDeviceService.cs',
        'Hipot_Tester/Services/DeviceService.cs', 
        'Hipot_Tester/Services/IDialogService.cs',
        'Hipot_Tester/Services/DialogService.cs',
        'Hipot_Tester/Devices/IDeviceFactory.cs',
        'Hipot_Tester/ViewModel/MainViewModel.cs',
        'Hipot_Tester/ViewModel/Control/RelayCommand.cs'
    ]
    
    total_errors = 0
    total_files = 0
    
    for file_path in cs_files:
        full_path = os.path.join(os.path.dirname(__file__), file_path)
        
        if not os.path.exists(full_path):
            print(f"❌ {file_path}: 파일을 찾을 수 없습니다")
            continue
        
        total_files += 1
        print(f"📁 {file_path}")
        
        errors = check_basic_syntax(full_path)
        
        if errors:
            for error in errors:
                print(f"  ⚠️  {error}")
            total_errors += len(errors)
        else:
            print("  ✅ 문법 오류 없음")
    
    print("\n" + "=" * 40)
    print("📊 검증 결과")
    print("=" * 40)
    print(f"검사 파일: {total_files}개")
    print(f"발견된 오류: {total_errors}개")
    
    if total_errors == 0:
        print("🎉 모든 파일의 기본 문법이 올바릅니다!")
        return 0
    else:
        print("⚠️  일부 문법 오류가 발견되었습니다.")
        return 1

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)