Imports System.IO
Imports Szunyi.IO
Imports Szunyi.IO.FilePath_Conversion
Public Class Form1
    Private Sub CreateKAnalyseScriptToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CreateKAnalyseScriptToolStripMenuItem.Click
        ' Get Files
        ' Get Kmer Length
        ' Get Output Directory
        Dim Files = Szunyi.IO.Pick_Up.Fasta_Fastq().ToList
        If Files.Count = 0 Then Exit Sub
        Dim Dir = Szunyi.IO.Pick_Up.Directory("Select Output Directory")
        If IsNothing(Dir) = True Then Exit Sub
        Dim Kmers = Szunyi.Common.MyInputBox.GetIntegers("Enter interesting Kmer lengths Separated By space")
        If Kmers.Count = 0 Then Exit Sub

        Dim str As New System.Text.StringBuilder
        For Each Kmer In Kmers
            For Each File In Files
                str.Append("count ").Append(File.FullName_Windows_cmd).Append(" -k ").Append(Kmer).Append(" -o ")
                Dim oFile = Szunyi.IO.Rename.Merge(Dir, File.Name).ChangeExtension(".kc")
                oFile = oFile.Append_Before_Extension("_" & Kmer)
                str.Append(oFile.FullName_Windows_cmd).AppendLine()
            Next
        Next
        Clipboard.SetText(str.ToString)
    End Sub

    Private Sub MergeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MergeToolStripMenuItem.Click
        ' Get Files
        ' Check Same Size
        ' Do
        Dim Files = Szunyi.IO.Pick_Up.Files(Szunyi.IO.File_Extensions.KAnalyse).ToList
        Dim FIle = Szunyi.IO.Export.File_To_Save()
        Dim x As New KAnalyse(Files, FIle)
        x.DoIt()
    End Sub

    Private Sub FilterToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FilterToolStripMenuItem.Click
        Dim x As New KAnalyseFilterSetting
        Dim t As New Szunyi.IO.Controls.Set_Console_Properties(x)
        If t.ShowDialog() <> DialogResult.OK Then Exit Sub
        Dim Files = Szunyi.IO.Pick_Up.Files(Szunyi.IO.File_Extensions.All_TAB_Like, "Every File is different Group. FileNames in File are separated by Enter without Header").ToList
        If Files.Count = 0 Then Exit Sub
        Dim File = Szunyi.IO.Pick_Up.File(Szunyi.IO.File_Extensions.KAnalyse)
        If IsNothing(File) = True Then Exit Sub
        Dim Header = Szunyi.IO.Import.Text.GetHeader(File, 1)
        Dim gr As New Groups(Header, Files)
        For Each Line In Szunyi.IO.Import.Text.ParseNotFirst(File, 1)
            Dim c = gr.GetCounts(Line)
            If x.MinSumInAll.Default_Value <= c.AllCounts Then
                For i1 = 0 To c.GroupCounts.Count - 1
                    ' Check is Any of Group is passed
                    If c.GroupCounts(i1) >= x.MinSumInGroup.Default_Value Then
                        If c.GroupPercents(i1) >= x.MinPercentOfGroup.Default_Value Then
                            If c.nofPositiveSampleInGroup(i1) >= x.MinPositiveInGroup.Default_Value Then
                                gr.SWs(i1).Write(">" & c.ToName(i1) & vbCrLf & c.Seq & vbCrLf)
                            End If
                        End If
                    End If
                Next
            End If
        Next
        For Each sw In gr.SWs
            sw.Flush()
            sw.Close()

        Next
        Dim kj As Int16 = 54


    End Sub
    Private Class Groups
        Private header As List(Of String)
        Private files As List(Of FileInfo)
        Public GroupIndexes As New List(Of Group)
        Public Property SWs As New List(Of StreamWriter)
        Public Sub New(header As List(Of String), files As List(Of FileInfo))
            Me.header = header
            Me.files = files
            Dim cIndex As Integer = 0
            Dim GrIndex As Integer = 0
            For Each File In files
                SWs.Add(New StreamWriter(File.ChangeExtension(Szunyi.IO.File_Extension.fasta).FullName))
                Dim GrName = File.woExtension
                Dim FileNames = Szunyi.IO.Import.Text.Parse(File).ToList
                For i1 = 1 To FileNames.Count
                    Dim g As New Group(i1 + cIndex, FileNames(i1 - 1), GrName.Name, GrIndex)
                    GroupIndexes.Add(g)
                    GrIndex += 1
                Next
                cIndex += FileNames.Count
            Next

        End Sub
        Public Function GetCounts(Line As String) As Counts
            Dim t = Split(Line, vbTab)
            Dim c As New Counts
            c.Seq = t.First
            ReDim c.GroupCounts(GroupIndexes.Count - 1)
            ReDim c.GroupPercents(GroupIndexes.Count - 1)
            ReDim c.nofPositiveSampleInGroup(GroupIndexes.Count - 1)
            For Each Item In Me.GroupIndexes
                c.GroupCounts(Item.GroupIndex) += t(Item.Index)
                If t(Item.Index) <> 0 Then c.nofPositiveSampleInGroup(Item.GroupIndex) += 1
            Next
            c.AllCounts = c.GroupCounts.Sum
            For i1 = 0 To GroupIndexes.Count - 1
                c.GroupPercents(i1) = c.GroupCounts(i1) / c.AllCounts * 100
            Next
            Return c
        End Function
    End Class
    Private Class Group

        Public Sub New(Index As Integer, FileName As String, GroupName As String, GrIndex As Integer)
            Me.Index = Index 'ColumnIndex
            Me.GroupName = GroupName
            Me.FileName = FileName
            Me.GroupIndex = GrIndex
        End Sub

        Public Property Index As Integer ' ColumnNumber
        Public Property GroupName As String
        Public Property GroupIndex As Integer
        Public Property FileName As String
    End Class

    Private Class Counts
        Public Property Seq As String
        Public Property AllCounts As Integer
        Public Property GroupCounts As Integer()
        Public Property GroupPercents As Double()
        Public Property nofPositiveSampleInGroup As Integer()
        Public Function ToName(Index As Integer) As String
            Return Seq & "_" & GroupCounts(Index) & "_" & GroupPercents(Index) & "_" & nofPositiveSampleInGroup(Index)
        End Function

    End Class
End Class
