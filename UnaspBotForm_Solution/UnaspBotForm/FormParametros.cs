using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.IO;

namespace UnaspBotForm
{
    public partial class FormParametros : Form
    {
        public FormParametros()
        {
            InitializeComponent();
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            Usuario usuario = new Usuario();

            usuario.user = txtUser.Text;
            usuario.password = txtSenha.Text;

            string json = new JavaScriptSerializer().Serialize(usuario);

            //write string to file
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "parametros.json", json);
            MessageBox.Show(null,"Dados salvos!", "Notificação", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void btnVerificar_Click(object sender, EventArgs e)
        {
            string json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "parametros.json");

            dynamic fileParametros = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            string usuario = fileParametros["user"];

            string senha = fileParametros["password"];

            MessageBox.Show(null, "Usuário: " + usuario + "\nSenha: " + senha + "", "Notificação", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void txtUser_Leave(object sender, EventArgs e)
        {
            label1.Visible = false;
        }

        private void txtUser_Enter(object sender, EventArgs e)
        {
            label1.Visible = true;
        }

        private void txtSenha_Leave(object sender, EventArgs e)
        {
            label2.Visible = false;
        }

        private void txtSenha_Enter(object sender, EventArgs e)
        {
            label2.Visible = true;
        }

        private void FormParametros_Load(object sender, EventArgs e)
        {

        }
    }
}
