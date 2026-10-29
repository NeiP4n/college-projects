Imports System.Drawing,
System.Drawing.Drawing2D,
System.Drawing.Imaging,
System.Drawing.Text
Public Class Form1

    Dim R As Graphics
    Dim P1 As Pen = New Pen(Color.Blue, 2)
    Private Sub Form1_Paint(ByVal sender As System.Object,
                            ByVal e As System.Windows.Forms.PaintEventArgs) Handles MyBase.Paint

        Dim P2 As Pen = New Pen(Color.Red, 2)
        R = Me.CreateGraphics
        R.DrawLine(P2, 10, 15, 110, 115)

        'Обьект Pen'

        Dim G As Graphics
        G = Me.CreateGraphics
        Dim f As New Font("Times New Roman", 20,
            FontStyle.Bold)
        Dim B As New SolidBrush(Color.Green)
        G.DrawString("Выводим текст", f, B, 100, 60)
        Dim P As Pen = New Pen(Color.Red, 1)
        P.DashStyle = DashStyle.Solid
        P.EndCap = LineCap.ArrowAnchor
        P.StartCap = LineCap.ArrowAnchor
        G.DrawLine(P, 100, 0, 100, 60)

        'Обьект Brush'

        Dim D As New SolidBrush(Color.Magenta)
        G.FillRectangle(D, 20, 40, 100, 50)
        Dim L As HatchBrush
        L = New HatchBrush(HatchStyle.DarkDownwardDiagonal,
            Color.Black, Color.White)
        G.FillEllipse(L, 130, 40, 100, 50)
        Dim S As New Pen(Color.Black, 3)
        G.DrawEllipse(S, 130, 40, 100, 50)
        G.DrawEllipse(Pens.Black, 250, 10, 50, 100)
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object,
                              ByVal e As System.EventArgs) Handles Button1.Click
        R = Me.CreateGraphics
        R.DrawLine(P1, 10, 165, 160, 165)



    End Sub
End Class
