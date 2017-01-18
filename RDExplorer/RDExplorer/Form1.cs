using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDExplorer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            menuStrip1.Renderer = new SelectionRenderer();
            contextMenuStrip1.Renderer = new SelectionRenderer();
        }

        // MENUSTRIP RENDERER
        private class SelectionRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs myMenu)
            {
                if (!myMenu.Item.Selected)
                    base.OnRenderMenuItemBackground(myMenu);
                else
                {
                    Rectangle menuRectangle = new Rectangle(Point.Empty, myMenu.Item.Size);
                    myMenu.Graphics.FillRectangle(Brushes.DodgerBlue, menuRectangle);
                    myMenu.Graphics.DrawRectangle(Pens.DeepSkyBlue, 1, 0, menuRectangle.Width - 2, menuRectangle.Height - 1);
                }
            }
        }

        // STRINGS BOOLS LISTS ETC
        public static string CurrentPath { get; private set; }
        List<string> listFiles = new List<string>();
        List<string> processFiles = new List<string>();
        public static string processCommand { get; private set; }
        public static bool pathFocused { get; private set; }

        // ON LOAD
        private void Form1_Load(object sender, EventArgs e)
        {
            CurrentPath = "C:\\Users\\" + Environment.UserName + "\\Desktop";
            button1.PerformClick();
            Explorer.Select();

            string[] drives = Environment.GetLogicalDrives();

            foreach (string drive in drives)
            {
                DriveInfo di = new DriveInfo(drive);
                int driveImage;

                switch (di.DriveType)    //set the drive's icon
                {
                    case DriveType.CDRom:
                        driveImage = 3;
                        break;
                    case DriveType.Network:
                        driveImage = 6;
                        break;
                    case DriveType.NoRootDirectory:
                        driveImage = 8;
                        break;
                    case DriveType.Unknown:
                        driveImage = 8;
                        break;
                    default:
                        driveImage = 2;
                        break;
                }

                TreeNode node = new TreeNode(drive.Substring(0, 1), driveImage, driveImage);
                node.Tag = drive;

                if (di.IsReady == true)
                    node.Nodes.Add("...");

                treeView1.Nodes.Add(node);
            }
            foreach (TreeNode expandRoot in treeView1.Nodes)
            {
                expandRoot.Expand();
            }
        }

        // EXPLORER EVENTS
        private void Explorer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                button2.PerformClick();
            }
            if (e.KeyCode == Keys.F5)
            {
                button1.PerformClick();
            }
        }

        //BACK
        private void button2_Click(object sender, EventArgs e)
        {
            if (CurrentPath == "C:\\")
            {
                CurrentPath = CurrentPath;
            }
            else
            {
                CurrentPath = Path.GetDirectoryName(CurrentPath);
                button1.PerformClick();
            }
        }

        // PATH TEXTBOX
        private void PathTextbox_Click(object sender, EventArgs e)
        {
            if (pathFocused == false)
            {
                pathFocused = true;
                PathTextbox.SelectAll();
            }
            else if (pathFocused == true)
            {
                pathFocused = false;
            }
        }
        private void PathTextbox_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(PathTextbox.Text))
            {
                CurrentPath = PathTextbox.Text;
                if (CurrentPath != "C:")
                {
                    button1.PerformClick();
                }
            }
        }

        // HOME
        private void button3_Click(object sender, EventArgs e)
        {
            CurrentPath = "C:\\Users\\" + Environment.UserName;
            button1.PerformClick();
        }

        // TREEVIEW BUILD
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count > 0)
            {
                if (e.Node.Nodes[0].Text == "..." && e.Node.Nodes[0].Tag == null)
                {
                    e.Node.Nodes.Clear();
                    string[] dirs = Directory.GetDirectories(e.Node.Tag.ToString());

                    foreach (string dir in dirs)
                    {
                        DirectoryInfo di = new DirectoryInfo(dir);
                        TreeNode node = new TreeNode(di.Name, 0, 1);

                        try
                        {
                            node.Tag = dir;

                            if (di.GetDirectories().Count() > 0)
                                node.Nodes.Add(null, "...", 0, 0);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            node.ImageIndex = 12;
                            node.SelectedImageIndex = 12;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "DirectoryLister",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            e.Node.Nodes.Add(node);
                        }
                    }
                }
            }
        }

        // CHANGE/REFRESH DIRECTORY
        private void button1_Click(object sender, EventArgs e)
        {
            PathTextbox.Text = CurrentPath;
            listFiles.Clear();
            Explorer.Items.Clear();
            foreach (string items in Directory.GetDirectories(CurrentPath))
            {
                if (Path.GetFileName(items) == "$Recycle.Bin")
                {
                    IconImageList.Images.Add(Image.FromFile("C:\\Program Files\\RDExplorer\\icons\\recyclebin.png"));
                }
                else
                {
                    IconImageList.Images.Add(Image.FromFile("C:\\Program Files\\RDExplorer\\icons\\folder.png"));
                }
                DirectoryInfo itemInfo = new DirectoryInfo(items);
                listFiles.Add(itemInfo.FullName);
                Explorer.Items.Add(itemInfo.Name, IconImageList.Images.Count - 1);
            }

            foreach (string items in Directory.GetFiles(CurrentPath))
            {
                if (Path.GetExtension(items) != ".lnk")
                {
                    if (Path.GetExtension(items) == ".png")
                    {
                        IconImageList.Images.Add(Image.FromFile(items));
                    }
                    else if (Path.GetExtension(items) == ".jpg")
                    {
                        IconImageList.Images.Add(Image.FromFile(items));
                    }
                    else if (Path.GetExtension(items) == ".ico")
                    {
                        IconImageList.Images.Add(Image.FromFile(items));
                    }
                    else
                    {
                        IconImageList.Images.Add(Icon.ExtractAssociatedIcon(items));
                    }
                    FileInfo itemInfo = new FileInfo(items);
                    listFiles.Add(itemInfo.FullName);
                    Explorer.Items.Add(itemInfo.Name, IconImageList.Images.Count - 1);
                }
            }
        }

        // CLICK
        private void Explorer_DoubleClick(object sender, EventArgs e)
        {
            if (Explorer.FocusedItem != null)
            {
                if (Directory.Exists(listFiles[Explorer.FocusedItem.Index]))
                {
                    CurrentPath = listFiles[Explorer.FocusedItem.Index];
                    button1.PerformClick();
                }
                else
                {
                    Process.Start(listFiles[Explorer.FocusedItem.Index]);
                }
            }
        }

        // CONTEXT MENU
        private void Explorer_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (Explorer.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        // CONTEXT MENU ITEMS
        //open
        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(listFiles[Explorer.FocusedItem.Index]))
            {
                CurrentPath = listFiles[Explorer.FocusedItem.Index];
                button1.PerformClick();
            }
            else
            {
                Process.Start(listFiles[Explorer.FocusedItem.Index]);
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openToolStripMenuItem1.PerformClick();
        }

        //exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //delete
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in Explorer.SelectedItems)
            {
                if (Directory.Exists(listFiles[item.Index]))
                {
                    Directory.Delete(listFiles[item.Index], true);
                }
                else
                {
                    if (File.Exists(listFiles[item.Index]))
                    {
                        File.Delete(listFiles[item.Index]);
                    }
                }
            }
            button1.PerformClick();
        }
        private void deleteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            deleteToolStripMenuItem.PerformClick();
        }

        //copy
        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            processFiles.Clear();
            foreach (ListViewItem item in Explorer.SelectedItems)
            {
                processFiles.Add(listFiles[item.Index]);
                processCommand = "copy";
            }
        }
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem1.PerformClick();
        }

        //cut
        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            processFiles.Clear();
            foreach (ListViewItem item in Explorer.SelectedItems)
            {
                processFiles.Add(listFiles[item.Index]);
                processCommand = "move";
            }
        }
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cutToolStripMenuItem1.PerformClick();
        }

        //paste
        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (processCommand == "copy")
            {
                foreach (string copyFiles in processFiles)
                {
                    if (File.Exists(copyFiles))
                    {
                        File.Copy(copyFiles, CurrentPath + "\\" + Path.GetFileName(copyFiles));
                    }
                    else
                    {
                        if (Directory.Exists(copyFiles))
                        {
                            if (!Directory.Exists(CurrentPath + "\\" + Path.GetFileName(copyFiles)))
                            Directory.CreateDirectory(CurrentPath + "\\" + Path.GetFileName(copyFiles));

                            foreach (string dirPath in Directory.GetDirectories(copyFiles, "*", SearchOption.AllDirectories))
                            {
                                Directory.CreateDirectory(dirPath.Replace(copyFiles, CurrentPath + "\\" + Path.GetFileName(copyFiles)));
                            }

                            foreach (string newPath in Directory.GetFiles(copyFiles, "*.*", SearchOption.AllDirectories))
                            {
                                File.Copy(newPath, newPath.Replace(copyFiles, CurrentPath + "\\" + Path.GetFileName(copyFiles)), true);
                            }
                        }
                    }
                }
                processFiles.Clear();
                button1.PerformClick();
            }
            if (processCommand == "move")
            {
                foreach (string moveFiles in processFiles)
                {
                    if (Directory.Exists(moveFiles))
                    {
                        Directory.Move(moveFiles, CurrentPath + "\\" + Path.GetFileName(moveFiles));
                    }
                    else
                    {
                        if (File.Exists(moveFiles))
                        {
                            File.Move(moveFiles, CurrentPath + "\\" + Path.GetFileName(moveFiles));
                        }
                    }
                }
                processFiles.Clear();
                button1.PerformClick();
            }
        }
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem1.PerformClick();
        }

        //select all
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in Explorer.Items)
            {
                item.Selected = true;
            }
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string treenodeselect = treeView1.SelectedNode.FullPath;
            treenodeselect = treenodeselect.Insert(1, ":");
            if (Directory.Exists(treenodeselect))
            {
                try
                {
                    CurrentPath = treenodeselect;
                    button1.PerformClick();
                }
                catch
                {
                    string nullstring = null;
                }
            }
        }
    }
}
