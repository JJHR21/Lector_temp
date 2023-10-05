using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lector_temp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        double temperatura = 0;
        double humdad = 0;
        bool actDato = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            //El botón abrir queda habilitado y se deshabilita el
            //botón cerrar
            btnAbrir.Enabled = true;
            btnCerrar.Enabled = false;
            //Se inicializan los puntos de la gráfica
            chart1.Series["Temperatura"].Points.AddXY(1, 1);
            chart1.Series["Humedad"].Points.AddXY(1, 1);
        }

        private void CBoxCOM_DropDown(object sender, EventArgs e)
        {
            //Obteniendo el nombre de los puertos activos
            string[] portList = SerialPort.GetPortNames();
            cmbCom.Items.Clear();
            //Agregandolos al combobox de puertos
            cmbCom.Items.AddRange(portList);
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            try
            {
                //Asignando el nombre del purrto seleccionado
                serialPort1.PortName = cmbCom.Text;
                //Convirtiendo a entero el valor del Baud Rate
                serialPort1.BaudRate = Convert.ToInt32(txtBaud.Text);
                //Abriendo la comunicación
                serialPort1.Open();
                //Se deshabilita el boton abrir y se
                //habilita el boton cerrar
                btnAbrir.Enabled = false;
                btnCerrar.Enabled = true;
                //Se limpian los puntos de la gráfica
                chart1.Series["Temperatura"].Points.Clear();
                chart1.Series["Humedad"].Points.Clear();

                MessageBox.Show("Conexion exitosa a la tarjeta arduino");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                //Se cierra la comunicación serial y se habilita el
                //botón de abrir
                serialPort1.Close();
                btnAbrir.Enabled = true;
                btnCerrar.Enabled = false;
                MessageBox.Show("Desconectado de la tarjeta arduino");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                serialPort1.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Se lee la cadena hasta encontrar el salto de linea
            string dataIn = serialPort1.ReadTo("\n");
            //Data parsin es un método que creamos para limpiar los datos
            //y al cual le pasaremos la cadena obtenida de la comuni_
            //cación serial
            Data_Parsing(dataIn);

            //Llamamos al método ShowData para que se despliegue la información obtenida del lector(ya limpia) esto se hará de forma 
            //asíncrona

            this.BeginInvoke(new EventHandler(Show_Data));
        }

        private void Show_Data(object sender, EventArgs e)
        {
            if (actDato == true)
            {
                //Si se recibe información se desplegará en el siguiente formato
                lblTemp.Text = string.Format("Temperatura= {0}°C", temperatura.ToString());
                lblHum.Text = string.Format("Humedad= {0}%HR",  humdad.ToString());
                //Se dibujan los puntos en la gráfica
                chart1.Series["Temperatura"].Points.Add(temperatura);
                chart1.Series["Humedad"].Points.Add(humdad);
            }
        }

        private void Data_Parsing(string dato)
        {
            //Se cran variables para ir encontrando los separadores
            //Que agregamos a la lectura en el código del arduino
            sbyte indexOf_startDataCharacter = (sbyte)dato.IndexOf("@");
            sbyte indexOfA = (sbyte)dato.IndexOf("A");
            sbyte indexOfB = (sbyte)dato.IndexOf("B");
            //Si el caracter es A o B, y la "@" existe in el paquete de  datos
            if (indexOfA != -1 && indexOfB != -1 && indexOf_startDataCharacter != -1)
            {
                try
                {
                    //Recuperamos el valor obtenidos de la temperatura

                    string str_Temperatura =
                    dato.Substring(indexOf_startDataCharacter + 1,
                    (indexOfA - indexOf_startDataCharacter) - 1);
                    //Recuperamos el valor obtenidos de la humedad
                    string str_humedad = dato.Substring(indexOfA + 1,

                    (indexOfB - indexOfA) - 1);
                    //Se guarda la información en las variables
                    temperatura = Convert.ToDouble(str_Temperatura);
                    humdad = Convert.ToDouble(str_humedad);
                    //Como si se recibe información se coloca la variable   en true
                    actDato = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                actDato = false;
            }
        }
    }
}
