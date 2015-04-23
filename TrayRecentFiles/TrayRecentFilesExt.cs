using System.Linq;
using KeePass.Plugins;
using System.Diagnostics;
using System.Windows.Forms;

namespace TrayRecentFiles {
    public sealed class TrayRecentFilesExt : Plugin {
        private IPluginHost m_host = null;
        private int m_menuFileRecentPreviousIndex = -1;
        
        private ToolStripMenuItem m_menuFile;
        private ToolStripMenuItem m_menuFileRecent;

        public override bool Initialize(IPluginHost host) {
            if (host == null) 
                return false;

            m_host = host;

            var mainWindow = m_host.MainWindow; // Alias for m_host.MainWindow
            if (mainWindow == null) return false;

            var mainMenu = mainWindow.MainMenu; // Alias for m_host.MainWindow.MainMenu
            if (mainMenu == null) return false;

            var trayContextMenu = mainWindow.TrayContextMenu; // Alias for m_host.MainWindow.TrayContextMenu
            if (trayContextMenu == null) return false;

            var menuFile = mainMenu.Items.Find("m_menuFile", searchAllChildren: false);
            if (menuFile == null || menuFile.Length != 1 || !(menuFile[0] is ToolStripMenuItem)) return false;
            m_menuFile = menuFile[0] as ToolStripMenuItem;

            var recentMenuFileRecent = mainMenu.Items.Find("m_menuFileRecent", searchAllChildren: true);
            if (recentMenuFileRecent == null || recentMenuFileRecent.Length != 1 || !(recentMenuFileRecent[0] is ToolStripMenuItem)) return false;

            m_menuFileRecent = recentMenuFileRecent[0] as ToolStripMenuItem;
            m_menuFileRecentPreviousIndex = mainMenu.Items.IndexOf(m_menuFileRecent);
            if (m_menuFileRecentPreviousIndex < 0) 
                m_menuFileRecentPreviousIndex = 2;

            m_menuFile.DropDownOpening += m_menuFile_DropDownOpening;
            trayContextMenu.Opening += contextMenuStrip_Opening;
            m_menuFileRecent.DropDownOpening += m_menuFileRecent_DropDownOpening;

            return true;
        }

        void m_menuFile_DropDownOpening(object sender, System.EventArgs e) {
            if (m_host == null || m_host.MainWindow == null || m_menuFileRecent == null)
                return;

            // A ToolStripMenuItem can only be on one place at once
            // The user is opening the "File" menu on Main Window, we will place it there
            // (removing it from the systray context menu)

            if (m_menuFile.DropDownItems.Contains(m_menuFileRecent))
                return;
            
            m_menuFile.DropDownItems.Insert(m_menuFileRecentPreviousIndex, m_menuFileRecent);
        }

        void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
            if (m_host == null || m_host.MainWindow == null || m_menuFileRecent == null)
                return;

            // A ToolStripMenuItem can only be on one place at once
            // The user is opening the systray context menu, we will place it there
            // (removing it from the "File" menu on Main Window)

            var trayContextMenu = m_host.MainWindow.TrayContextMenu;
            if (trayContextMenu.Items.Contains(m_menuFileRecent)) 
                return;

            trayContextMenu.Items.Insert(0, m_menuFileRecent); // On top seems to be as good a location as any other
        }

        void m_menuFileRecent_DropDownOpening(object sender, System.EventArgs e) {
            if (m_menuFileRecent == null)
                return;

            // After the user clicks any recent item, make sure the main window is visible
            foreach (var mnuItem in m_menuFileRecent.DropDownItems.OfType<ToolStripMenuItem>()) {
                mnuItem.Click -= EnsureMainWindowVisible;
                mnuItem.Click += EnsureMainWindowVisible;
            }
        }

        void EnsureMainWindowVisible(object sender, System.EventArgs e) {
            if (m_host != null && m_host.MainWindow != null)
                m_host.MainWindow.EnsureVisibleForegroundWindow(bUntray: true, bRestoreWindow: true);
        }

        public override void Terminate() {
            if (m_host != null)
                m_host.MainWindow.TrayContextMenu.Opening -= contextMenuStrip_Opening;                
            
            if (m_menuFile != null)
                m_menuFile.DropDownOpening -= m_menuFile_DropDownOpening;
            
            if (m_menuFileRecent != null)
                foreach (ToolStripItem mnuItem in m_menuFileRecent.DropDownItems)
                    mnuItem.Click -= EnsureMainWindowVisible;
        }
    }
}
