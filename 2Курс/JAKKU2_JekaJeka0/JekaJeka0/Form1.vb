Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        TextBox1.Text = "Начинаем работу"
        HScrollBar1.Width = 70
        Button2.BackColor = Color.Yellow
        RadioButton2.Hide()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        HScrollBar1.Width = 180
        TextBox1.Text = "Ширина полосы = 180"
        RadioButton2.Show()
        Button2.BackColor = Color.Red
        Me.BackColor = Color.Blue
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        HScrollBar1.Height = 10
        Me.BackColor = Color.Black
        TextBox1.Text = "Проект завершен"
    End Sub
End Class
