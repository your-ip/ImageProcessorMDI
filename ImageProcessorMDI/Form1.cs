using System;
using System.Windows.Forms;

namespace ImageProcessorMDI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Выберите изображение"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                CreateChildForm(openFileDialog.FileName, ProcessingMode.Fast);
            }
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild is ImageChildForm activeChild)
            {
                activeChild.SaveImage();
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void CreateChildForm(string imagePath, ProcessingMode mode)
        {
            ImageChildForm childForm = new ImageChildForm(imagePath, mode)
            {
                MdiParent = this
            };
            childForm.Show();
        }

        private void OpenSlowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Выберите изображение"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                CreateChildForm(openFileDialog.FileName, ProcessingMode.Slow);
            }
        }
    }

    public enum ProcessingMode
    {
        Fast,
        Slow
    }
}