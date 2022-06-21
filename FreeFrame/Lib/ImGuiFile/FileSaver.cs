/**
 * Original Author: prime31
 * Source: https://gist.github.com/prime31/91d1582624eb2635395417393018016e
 */
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.IO;
using Num = System.Numerics;

namespace FreeFrame.Lib.FileExplorer
{
    public class FileSaver
    {
        static readonly Dictionary<object, FileSaver> _fileSavers = new Dictionary<object, FileSaver>();

        public string RootFolder;
        public string CurrentFolder;

        public string Filename;

        public static FileSaver GetFilePicker(object o, string startingPath)
        {
            if (File.Exists(startingPath))
            {
                startingPath = new FileInfo(startingPath).DirectoryName;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                    startingPath = AppContext.BaseDirectory;
            }

            if (!_fileSavers.TryGetValue(o, out FileSaver fp))
            {
                fp = new FileSaver
                {
                    RootFolder = startingPath,
                    CurrentFolder = startingPath,
                    Filename = string.Empty,
                };

                _fileSavers.Add(o, fp);
            }

            return fp;
        }
        public static void Clear() => _fileSavers.Clear();

        public bool Draw()
        {
            if (CurrentFolder.Length >= 50)
                ImGui.Text("Current Folder: " + CurrentFolder.Replace(CurrentFolder[10..(CurrentFolder.Length - 40)], "..."));
            else
                ImGui.Text("Current Folder: " + CurrentFolder);
            bool result = false;

            if (ImGui.BeginChildFrame(1, new Num.Vector2(400, 400)))
            {
                var di = new DirectoryInfo(CurrentFolder);
                if (di.Exists)
                {
                    if (di.Parent != null)
                        if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                            CurrentFolder = di.Parent.FullName;

                    var fileSystemEntries = GetFileSystemEntries(di.FullName);
                    foreach (var fse in fileSystemEntries)
                    {
                        if (Directory.Exists(fse))
                        {
                            var name = Path.GetFileName(fse);
                            ImGui.PushStyleColor(ImGuiCol.Text, new Num.Vector4(0, 125, 255, 255));
                            if (ImGui.Selectable($"[D] {name}/", false, ImGuiSelectableFlags.DontClosePopups))
                                CurrentFolder = fse;
                            ImGui.PopStyleColor();
                        }
                    }
                }
            }
            ImGui.EndChildFrame();


            if (ImGui.Button("Cancel"))
            {
                result = false;
                ImGui.CloseCurrentPopup();
                Clear();
            }
            ImGui.SameLine();
            if (ImGui.Button("Save here"))
            {
                result = true;
                ImGui.CloseCurrentPopup();
            }
            ImGui.InputText("Filename", ref Filename, 64);

            return result;
        }
        List<string> GetFileSystemEntries(string fullName)
        {
            var files = new List<string>();
            var dirs = new List<string>();

            foreach (var fse in Directory.GetFileSystemEntries(fullName, ""))
                if (Directory.Exists(fse))
                    dirs.Add(fse);

            var ret = new List<string>(dirs);
            ret.AddRange(files);

            return ret;
        }

    }
}