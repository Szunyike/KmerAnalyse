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
End Class
