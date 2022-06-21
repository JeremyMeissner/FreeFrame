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
    public class FilePicker
    {
        static Dictionary<object, FilePicker> _filePickers = new Dictionary<object, FilePicker>();

        public string RootFolder;
        public string CurrentFolder;
        public string SelectedFile;
        public List<string> AllowedExtensions;
        public bool OnlyAllowFolders;

        public static FilePicker GetFolderPicker(object o, string startingPath)
            => GetFilePicker(o, startingPath, null, true);

        public static FilePicker GetFilePicker(object o, string startingPath, string searchFilter = "", bool onlyAllowFolders = false)
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

            if (!_filePickers.TryGetValue(o, out FilePicker fp))
            {
                fp = new FilePicker
                {
                    RootFolder = startingPath,
                    CurrentFolder = startingPath,
                    OnlyAllowFolders = onlyAllowFolders
                };

                if (fp.AllowedExtensions != null)
                    fp.AllowedExtensions.Clear();
                else
                    fp.AllowedExtensions = new List<string>();

                fp.AllowedExtensions.AddRange(searchFilter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

                _filePickers.Add(o, fp);
            }
            return fp;
        }

        public static void Clear() => _filePickers.Clear();

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
                    if (di.Parent != null) // && CurrentFolder != RootFolder
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
                        else
                        {
                            var name = Path.GetFileName(fse);
                            bool isSelected = SelectedFile == fse;
                            if (ImGui.Selectable($"[F] {name}", isSelected, ImGuiSelectableFlags.DontClosePopups))
                                SelectedFile = fse;

                            if (ImGui.IsMouseDoubleClicked(0))
                            {
                                if (isSelected)
                                {
                                    result = true;
                                    ImGui.CloseCurrentPopup();
                                }
                            }
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

            if (OnlyAllowFolders)
            {
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    SelectedFile = CurrentFolder;
                    ImGui.CloseCurrentPopup();
                }
            }
            else if (SelectedFile != null)
            {
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    ImGui.CloseCurrentPopup();
                }
            }

            return result;
        }
        List<string> GetFileSystemEntries(string fullName)
        {
            var files = new List<string>();
            var dirs = new List<string>();

            foreach (var fse in Directory.GetFileSystemEntries(fullName, ""))
            {
                if (Directory.Exists(fse))
                {
                    dirs.Add(fse);
                }
                else if (!OnlyAllowFolders)
                {
                    if (AllowedExtensions != null)
                    {
                        if (AllowedExtensions.Contains(Path.GetExtension(fse)))
                            files.Add(fse);
                    }
                    else
                        files.Add(fse);
                }
            }

            var ret = new List<string>(dirs);
            ret.AddRange(files);

            return ret;
        }
    }
}