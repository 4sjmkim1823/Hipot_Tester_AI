   M S B A M L   `   `   `  ����G  CHipot_Tester, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null?$clr-namespace:Hipot_Tester.ViewModelHipot_Tester.ViewModel  5   	   +    %Hipot_Tester.View.DeviceSelectionView   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   3vm$clr-namespace:Hipot_Tester.ViewModel     5   	     � Title$  장비 선택��5   	   $	��250��5   	   $	��400��5   	    � WindowStartupLocation$ CenterScreen=�5   	    � 
ResizeMode$ NoResize=�5	   	    � 	Resources 5       �� MergedDictionaries �� Source�� 5   
    5      �� 5      $% /View/LGResourceDictionary.xaml?�6%   %    5       � DataContext 5      5   /Hipot_Tester.ViewModel.DeviceSelectionViewModel  5   
   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5    
   �� $��0��6   $��Hipot Tester��5!      $��22(�5"      $
��Bold"�5#      $��Center=�5$      $��	0,20,0,10q�5%      �� 5(   
   $��1��6   $,��&테스트할 장비를 선택하세요��5)      $��16(�5*      $��Center=�5+      $��0,0,0,20q�5,      �� 5/   
   $��2��6      LG.ComboBox#��[  51      $��Center=�53      $	��200��54      $��0,0,0,20q�55      ��50      �� *AvailableDevices+ �� SelectedItem 52      �� *SelectedDevice+55      �� 58   
   $��3��6   	 �� Orientation$	 
Horizontal=�59      $��Center=�5:      .+�5;      �� $��시작��6     LG.ButtonPrimary#��[ 5<      $	��120��5>      $��40��5?      $��10q�5@      ��5=      �� *StartTestCommand+5@      �� 5B      $��취소��6     LG.ButtonSecondary#��[ 5C      $	��120��5E      $��40��5F      $��10q�5G      ��5D      �� *CancelCommand+5G      5H      5I      5J      