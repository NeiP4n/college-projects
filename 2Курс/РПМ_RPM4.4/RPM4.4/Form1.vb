Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim a As Char = InputBox("Введите символ")
        Select Case a
            Case "А", "а", "Е", "е", "Ё", "ё", "И", "и", "О",
"о", "У", "у", "Э", "э", "Ю", "ю", "Я", "я"
                TextBox1.Text = "Гласная буква"

            Case "Ъ", "ъ"
                TextBox1.Text = "Твердый знак"

            Case "Ь", "ь"
                TextBox1.Text = "Мягкий знак"

            Case "К", "к", "П", "п", "С", "с", "Т", "т", "Ф",
"ф", "Х", "х", "Ц", "ц", "Ч", "ч", "Ш", "ш", "Щ", "щ"
                TextBox1.Text = "Согласный глухой"

            Case "Б", "б", "В", "в", "Г", "г", "Д", "д", "Ж",
"ж", "З", "з", "Л", "л", "М", "м", "Н", "н", "Р", "р"

                TextBox1.Text = "Согласный звонкий"
            Case Else
                TextBox1.Text = "Не знаю"
        End Select

    End Sub


    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub
End Class
