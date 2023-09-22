Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Windows.Forms

Public Class MainForm
    '*******************************************************************************
    '* Stop polling when the form is not visible in order to reduce communications
    '* Copy this section of code to every new form created
    '*******************************************************************************
    Private NotFirstShow As Boolean
    Private Sub Form_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.VisibleChanged
        '* Do not start comms on first show in case it was set to disable in design mode
        If NotFirstShow Then
            AdvancedHMIDrivers.Utilities.StopComsOnHidden(components, Me)
        Else
            NotFirstShow = True
        End If
    End Sub
#Region "PLC_TAGS"
    'List of PLC TAGS
    Private PLC_TAGS_BOOL As New List(Of AdvancedHMI.Drivers.PLCAddressItem)
    Private PLC_TAGS_INTERGER As New List(Of AdvancedHMI.Drivers.PLCAddressItem)
    Private PLC_TAGS_FLOAT As New List(Of AdvancedHMI.Drivers.PLCAddressItem)

    Private PLC_TAGS As New List(Of AdvancedHMI.Drivers.PLCAddressItem)

#End Region
    Private TagsFromFile As String() = New String() {}
    Private fileName As String = My.Computer.FileSystem.SpecialDirectories.Desktop + "\tags.txt"
    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        RichTextBox1.AppendText($"[{Date.Now.ToString("G")}]:MainForm_Load.{vbLf}")
        'Get tag names from text file.
        If System.IO.File.Exists(fileName) Then
            TagsFromFile = System.IO.File.ReadAllLines(fileName)
        End If
        'Check if file has tags.
        If TagsFromFile.Length > 0 Then
            For Each line As String In TagsFromFile
                RichTextBox1.AppendText($"[{Date.Now.ToString("G")}]:Tag from file:{line}{vbNewLine}")
                PLC_TAGS.Add(New Drivers.PLCAddressItem(line))
            Next line
        End If
#Region "Add plc tags"
        PLC_TAGS.Add(New Drivers.PLCAddressItem("AdvancedHMI_BOOL"))
        PLC_TAGS.Add(New Drivers.PLCAddressItem("AdvancedHMI_BOOL_Reset"))
        PLC_TAGS.Add(New Drivers.PLCAddressItem("AdvancedHMI_BOOL_Reset_Value"))
        PLC_TAGS.Add(New Drivers.PLCAddressItem("AdvancedHMI_INT"))
        PLC_TAGS.Add(New Drivers.PLCAddressItem("AdvancedHMI_REAL"))
#Region "Bind the PLCAddressItems to the Subscribers"
        For Each Item In PLC_TAGS
            DataSubscriber21.PLCAddressValueItems.Add(Item)
        Next Item
#End Region
#End Region
    End Sub
#Region "DataChanged Handles"
    Private Sub DataSubscriber21_DataChanged(sender As Object, e As Drivers.Common.PlcComEventArgs) Handles DataSubscriber21.DataChanged
        If e.ErrorId > 0 Then
            ' MsgBox($"{sender.ToString}:{e.Values}")
            RichTextBox1.AppendText($"{sender.ToString}:{e.Values}{vbLf}")
        End If
        If e.ErrorId = 0 Then
            If e.Values IsNot Nothing AndAlso e.Values.Count > 0 Then
                If e.PlcAddress = "AdvancedHMI_BOOL" Then
                    Label_Bool.Text = e.Values(0)
                ElseIf e.PlcAddress = "AdvancedHMI_REAL" Then
                    UpDateValue(e.Values(0))
                ElseIf e.PlcAddress = "AdvancedHMI_BOOL_Reset_Value" Then

                ElseIf e.PlcAddress = "AdvancedHMI_BOOL_Reset" Then
                    If e.Values(0) Is "True" Then
                        Try
                            EthernetIPforCLXCom1.Write("AdvancedHMI_BOOL_Reset_Value", NumericUpDown1.Value)
                            EthernetIPforCLXCom1.Write("AdvancedHMI_BOOL_Reset", 0)
                        Catch ex As Exception
                            RichTextBox1.AppendText($"[{Date.Now.ToString("G")}]:{ex.Source}:{ex.Message}")
                        End Try
                    End If
                ElseIf e.PlcAddress = "AdvancedHMI_INT" Then
                    Label_INT.Text = e.Values(0)
                End If
            End If
        End If
    End Sub
#End Region
#Region "sub/function"
    Private Sub UpDateValue(ByVal val As String)
        '
        Dim m As Match = Regex.Match(val, "\d{1,4}.\d{1,3}", RegexOptions.Singleline)
        If m.Success Then
            Label_Real.Text = $"{m.Value}"
        End If
    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        'Navigate to a URL.
        System.Diagnostics.Process.Start("https://advancedhmi.com/documentation/index.php/DataSubscriber")
    End Sub

    Private Sub BasicDataLogger1_DataChanged(sender As Object, e As Drivers.Common.PlcComEventArgs)

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            EthernetIPforCLXCom1.Write("AdvancedHMI_BOOL_Reset", 1)
        Catch ex As Exception
            RichTextBox1.AppendText($"[{Date.Now.ToString("G")}]:{ex.Source}:{ex.Message}")
        End Try
    End Sub
#End Region
End Class
