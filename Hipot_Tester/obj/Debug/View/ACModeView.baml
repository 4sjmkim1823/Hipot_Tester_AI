   M S B A M L   `   `   `  ����G  CHipot_Tester, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:Hipot_Tester.ViewHipot_Tester.View  5      "    Hipot_Tester.View.ACModeView   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      1localclr-namespace:Hipot_Tester.View     5      .��5
      �� $��0,0,10,0q�6   ���	#FF4A4A4A$��2q�5        �� CornerRadius$  5��5      ._�5   
   � ���	#FF1E2A38 � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $��* �6    .+�5      �� $��AC MODE 장비 설정��6      StyleTextBlockTitle1#��[  5      ���	#FF2E2E2E$��	0 10 0 10q�5      �� 5      $��1��6   .+�5      �� $
��VOLT��6   
�������$��16(�5      $��arial)�5      $
��10,5q�5      � 5       � ColumnDefinitions 5       �� 5!      $
��0.8* �6+   �� 5"      $��2* �6+   �� 5#      $��1* �6+    5%      �� 5&      $��* �6(   �� 5'      $��* �6(   .+�5*      �� $��0��6   $��1��5+        digitalfont#��[ 5,        StyleMeasureResultTextBox#��[ 5-      ��5.      �� *ACVolt+ �� UpdateSourceTrigger$ PropertyChanged=��� 5/      $�� V��6    $��0��50       $��2��51       
�������$��20(�53       $��Center=�54       55      � 56       57      �� 58      $
��0.8* �6+   �� 59      $��2* �6+   �� 5:      $��1* �6+    5<      �� 5=      $��* �6(   �� 5>      $��* �6(   .+�5@      �� $
��HIGH��6    $��0��5A       
�������$
��10,5q�5C       $��arial)�5D       $��16(�5E       �� 5F      $��1��6   $��1��5G      #��[ 5H      #��[ 5I      ��5J      �� *ACVolt+$ PropertyChanged=��� 5K      $��mA��6    $��1��5L       $��2��5M       
�������$��20(�5O       $��Center=�5P       5Q      5R      5S      5T      5V      