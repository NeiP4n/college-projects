Public Class Form1
    ' Текущая цена топлива
    Dim DT As Double = 64.72
    Dim AI92 As Double = 50.56
    Dim AI95 As Double = 55.23
    Dim AI98 As Double = 68.66

    Private Sub btnCalculate_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' Вводим количество топлива в литрах
        Dim l As Double = InputBox("Введите количество топлива: ")

        ' Выражения
        Dim DC As Double = l * DT
        Dim AI92C As Double = l * AI92
        Dim AI95C As Double = l * AI95
        Dim AI98C As Double = l * AI98

        ' Для MessageBox
        Dim message As String = "Стоимость топлива за " & l.ToString() & " литров:" & vbCrLf &
        "ДТ: " & DC.ToString() & " ₽" & vbCrLf &
        "АИ-92: " & AI92C.ToString() & " ₽" & vbCrLf &
        "АИ-95: " & AI95C.ToString() & " ₽" & vbCrLf &
        "АИ-98: " & AI98C.ToString() & " ₽"

        ' Результаты в MessageBox
        MessageBox.Show(message, "Расчет стоимости топлива")
        TextBox1.Text = message
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub
End Class
