   M S B A M L   `   `   `  ���� G  CHipot_Tester, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:Hipot_Tester.ViewHipot_Tester.View  ?$clr-namespace:Hipot_Tester.ViewModelHipot_Tester.ViewModel  $    Hipot_Tester.View.IR_1905XView   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     1localclr-namespace:Hipot_Tester.View     3vm$clr-namespace:Hipot_Tester.ViewModel       � DataContext  ,   &Hipot_Tester.ViewModel.IRModeViewModel  .���� $��0,0,10,0q����	#FF4A4A4A$��2q� �� CornerRadius$ 5��._�� ���	#FF1E2A38 � RowDefinitions �� $
��Auto ��� $
��Auto ��� $
��Auto ��� $��* �.+��� $��IR MODE 장비 설정��   StyleTextBlockTitle1#��[  ���	#FF2E2E2E$��	0 10 0 10q��� $��1��.+��� $
��VOLT��
�������$��16(�$��arial)�$
��10,5q��  � ColumnDefinitions �� $
��0.8* ��� $��2* ��� $��1* � �� $��* ��� $��* �.+��� -    ��Name$ VoltageTextBox��$��0��$��1��  digitalfont#��[   StyleMeasureResultTextBox#��[ ���� *Volt+ �� UpdateSourceTrigger$ PropertyChanged=� �� Mode$ TwoWay=� �� StringFormat$ N2���� $�� V��$��0��$��2��
�������$��20(�$��Center=��� $	��LOW��
�������$��16(�$��arial)�$
��10,5q��  �� $
��0.8* ��� $��2* ��� $��1* � �� $��* ��� $��* �.+��� -   $ ResistanceTextBox��$��0��$��1��#��[ #��[ ���� *Low+$ PropertyChanged=�$ TwoWay=�$ N2���� $�� MΩ��$��0��$��2��
�������$��20(�$��Center=��� $
��TIME��
�������$��16(�$��arial)�$
��10,5q��  �� $
��0.8* ��� $��2* ��� $��1* � �� $��* ��� $��* �.+��� $��0��$��1��#��[ #��[ ���� *Time+$ PropertyChanged=�$ TwoWay=�$ N2���� $�� S��$��0��$��2��
�������$��20(�$��Center=��  �� $
��0.8* ��� $��2* ��� $��1* � �� $��* ��� $��* �.+��� $
��HIGH��$��0��
�������$
��10,5q�$��arial)�$��16(��� $��1��$��1��#��[ #��[ ���� *High+$ PropertyChanged=�$ TwoWay=�$ N2���� $	�� mA��$��1��$��2��
�������$��20(�$��Center=��� $
��RAMP��
�������$��16(�$��arial)�$
��10,5q��  �� $
��0.8* ��� $��2* ��� $��1* � �� $��* ��� $��* �.+��� $��0��$��1��#��[ #��[ ���� *Ramp+$ PropertyChanged=�$ TwoWay=�$ N2���� $�� S��$��0��$��2��
�������$��20(�$��Center=��  �� $	��10* ��� $	��18* ��� $	��70* ��� $	��35* � �� $��* ��� $��* ��� $
��FALL��
�������$��16(�$��arial)�$
��10,5q��  �� $
��0.8* ��� $��2* ��� $��1* � �� $��* ��� $��* �.+��� $��0��$��1��#��[ #��[ ���� *Fall+$ PropertyChanged=�$ TwoWay=�$ N2���� $�� S��$��0��$��2��
�������$��20(�$��Center=��� $��RANGE��
�������$��16(�$��arial)�$
��10,5q��  �� $
��0.8* ��� $��2* ��� $��1* � �� $��* ��� $��* �.+��� $��0��$��1��  StyleMeasureResultCombo#��[ ���� *RangeOptions+ �� SelectedItem �� *SelectedRange+$ TwoWay=�