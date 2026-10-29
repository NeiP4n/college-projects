Public Class Form1
    Private GraphicsFun As System.Drawing.Graphics
    Private BrushColor As New SolidBrush(Color.Yellow)
    Dim i, x0, y0, x, y As Integer
    Dim k, b As Double
    Dim koordX(20), koordY(20) As Integer
    Dim urav(20, 20) As Double
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        PictureBox1.Refresh()
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        PictureBox1.Refresh()
        Randomize()
        GraphicsFun = PictureBox1.CreateGraphics
        x0 = PictureBox1.Width / 2
        y0 = PictureBox1.Height / 2
        For i = 0 To 20
            koordX(i) = Rnd() * PictureBox1.Width
            koordY(i) = Rnd() * PictureBox1.Height
            If koordX(i) = x0 Or koordY(i) = y0 Then
                koordX(i) = koordX(i) + 1
                koordY(i) = koordY(i) + 1
            End If
            GraphicsFun.FillEllipse(BrushColor, koordX(i), koordY(i), 5, 5)
        Next
    End Sub
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        PictureBox1.Refresh()
        For i = 0 To 20
            x = koordX(i)
            y = koordY(i)
            If (koordX(i) <> x0) Or (koordY(i) <> y0) Then
                If x > x0 Then
                    x = x - 1
                    y = x * urav(i, i) + urav(i, i + 1)
                    GraphicsFun.FillEllipse(BrushColor, x, y, 5, 5)
                    koordX(i) = x
                    koordY(i) = y
                End If
                If x < x0 Then
                    x = x + 1
                    y = x * urav(i, i) + urav(i, i + 1)
                    GraphicsFun.FillEllipse(BrushColor, x, y, 5, 5)
                    koordX(i) = x
                    koordY(i) = y
                End If
            Else
                GraphicsFun.FillEllipse(BrushColor, x, y, 5, 5)
            End If
        Next
    End Sub
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        For i = 0 To 20
            x = x0 - koordX(i)
            y = y0 - koordY(i)
            k = y / x
            b = y0 - x0 * k
            urav(i, i) = k
            urav(i, i + 1) = b
        Next
        x = 0
        y = 0
        Timer1.Start()
    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        Timer1.Stop()
    End Sub

   
End Class
