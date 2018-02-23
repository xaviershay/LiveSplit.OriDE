using LiveSplit.OriDE.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.OriDE
{
    public partial class OriGPS : Form
    {
		public OriMemory Memory { get; set; }

        public OriGPS()
        {
            InitializeComponent();

			Memory = new OriMemory();
			Thread t = new Thread(UpdateLoop);
			t.IsBackground = true;
			t.Start();
        }

        private void UpdateLoop() {
            bool lastHooked = false;
            while (true) {
                try {
                    bool hooked = Memory.HookProcess();
                    if (hooked) {
                        UpdateValues();
                    }
                    if (lastHooked != hooked) {
                        lastHooked = hooked;
                    }
                    Thread.Sleep(100);
                } catch { }
            }
        }
        public void UpdateValues()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)UpdateValues);
            }
            else
            {
                //PointF pos = tasEnabled && Memory.HasTAS() ? Memory.GetTASOriPositon() : Memory.GetCameraTargetPosition();
                PointF pos = Memory.GetCameraTargetPosition();
                mapPanel.CenterLocationInGame = pos;
            }
        }

        private void mapPanel_Scroll(object sender, ScrollEventArgs e)
        {
            Console.WriteLine(e.OldValue);
            Console.WriteLine(e.NewValue);
        }
    }
}
