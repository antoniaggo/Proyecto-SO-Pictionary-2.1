﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Runtime.InteropServices;

namespace Cliente_v2
{
    public partial class Cliente : Form
    {
        Socket server;
        Thread atender;
        string palabra;
        int contador = 60;
        bool creador;
        int puntuacion;
        string ganador;
        bool dibujante = false;
        int numsala;
        int pos_puntos_color = 0;
        int pos_puntos_borrar = 0;
        List<Point> puntos_color = new List<Point>();
        List<Point> puntos_borrar = new List<Point>();
        
        int a;
        int index;

        string messageReceived = "\0";
        string messageSended = "\0";

        Bitmap bm;
        Graphics g;
        bool paint = false;
        Point px, py;
        Pen pen = new Pen(Color.Black, 1);
        Pen erase = new Pen(Color.White, 50);
        
        ColorDialog cd = new ColorDialog();
        Color new_color;
        public Cliente()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            lista.Items.Add("Inicia sesión para ver quien esta conectado");
            bm =new Bitmap(pic.Width,pic.Height);
            g=Graphics.FromImage(bm);
            g.Clear(Color.White);
            pic.Image = bm;
            btn_pencil.Enabled = false;
            btn_eraser.Enabled = false;
            button3.Enabled = false;
            Consultas_menu.Enabled = false;
        }

        private void atenderServidor()
        {
            while (true)
            {
                //mensaje que llega desde el servidor
                byte[] msg2 = new byte[80];
                server.Receive(msg2);
                messageReceived = Encoding.ASCII.GetString(msg2).Trim('\0');
                if (messageReceived == "\0")
                {

                } else
                {

                
                string[] trozos = Encoding.ASCII.GetString(msg2).Split('/');
                int codigo = Convert.ToInt32(trozos[0]);
                string mensaje = trozos[1].Split('\0')[0];
                switch (codigo)
                {
                    case 1: //respuesta cuando intentamos iniciar sesión
                        /*Recibimos la respuesta del servidor, que sera SI o NO, ..., Si es si se mostrara un mensaje en pantalla y 
                        se pasara al formulario de consultas, si sale no saldra un mensaje con un error*/
                        MessageBox.Show(messageReceived);
                        if (mensaje == "SI") //MENSAJE TIENE QUE SER 'SI'
                        {
                            MessageBox.Show("Sesión Iniciada");
                            //para que el usuario no vuelva a inciar o registrarse, ya que ya estara registrado
                            iniciar_sesion.Visible = false;
                            registrarse.Visible = false;
                            numsala = -1;
                        }
                        else if (mensaje == "incorrectPass")
                        {
                            MessageBox.Show("Error al iniciar sesión, compruebe el nombre o la contraseña");

                        }
                        else if (mensaje == "noUser")
                        {
                            MessageBox.Show("Error al inciar sesión, no existe el usuario introducido");
                        }
                        else if (mensaje == "yaConectado")
                        {
                            MessageBox.Show("El usuario ya esta conectado a nuestro servidor");
                        }
                        break;
                    case 2: //respuesta cuando nos intentamos registrar
                        /*Recibimos la respuesta del servidor, que sera SI o NO, Si es si se mostrara un mensaje en pantalla y 
                        se pasara al formulario de inicio de sesion, si sale no saldra un mensaje con un error*/
                        MessageBox.Show(messageReceived);
                        if (mensaje == "SI") //MENSAJE TIENE QUE SER 'SI'
                        {
                            MessageBox.Show("Registro completado");
                        }
                        else if (mensaje == "NO")
                        {
                            MessageBox.Show("Error al completar el registro, pruebe con otro nombre o contraseña");
                        }
                        else if (mensaje == "EXISTE")
                        {
                            MessageBox.Show("El usuario que has introducido ya existe");
                        }
                        break;

                    case 3:
                        MessageBox.Show(messageReceived);
                        if (mensaje != "no resultados")
                            MessageBox.Show("El jugador es el siguiente: " + mensaje);//Mensaje sera el nombre del escenario
                        else
                            MessageBox.Show("No hay resultados");
                        break;
                    case 4:
                        MessageBox.Show(messageReceived);
                        if (mensaje != "no resultados")
                            MessageBox.Show("El jugador es el siguiente: " + mensaje);//Mensaje tiene el nombre del jugador ganador
                                                                                                      //aqui se podria implementar una tabla utilizando un grid
                        else
                            MessageBox.Show("No hay resultados");
                        break;
                    case 5:
                        MessageBox.Show(messageReceived);
                        if (mensaje != "no resultados")
                            MessageBox.Show("El jugador es el siguiente: " + mensaje);
                        else
                            MessageBox.Show("No hay resultados");

                        //Mensaje debera tener un formato JugadorPerdedor-JugadorGanador
                        break;
                    case 6: //lista de usuarios conectados
                        MessageBox.Show(messageReceived);
                        int num = Convert.ToInt32(trozos[1]);
                        int i = 0;
                        lista.Items.Clear();
                        while (i < num)
                        {
                            lista.Items.Add(trozos[i + 2]);
                            i++;
                        }
                        break;
                    case 7://crear la sala
                        MessageBox.Show(messageReceived);
                        if (trozos[1] == "Creada")
                        {
                            MessageBox.Show("Sala Creada");//Mensaje tiene el nombre del jugador ganador
                            numsala = Convert.ToInt32(trozos[2]);
                        }
                        //aqui se podria implementar una tabla utilizando un grid
                        else
                            MessageBox.Show("Sala no creada");
                        break;
                    case 8://invitacio
                        MessageBox.Show(messageReceived);
                        if (mensaje == "Enviada")
                        {
                            DialogResult result = MessageBox.Show("¿Deseas aceptar la invitación?", "Invitación", MessageBoxButtons.OKCancel);

                            // Verificar qué botón se presionó
                            if (result == DialogResult.OK)
                            {
                                // Lógica para aceptar la invitación
                                string msgEnvio = "9/" + textUsuario.Text + "/" + trozos[2];
                                // Enviamos al servidor la consulta para la DB
                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(msgEnvio);
                                server.Send(msg);
                                numsala = Convert.ToInt32(trozos[3]);
                            }
                            else if (result == DialogResult.Cancel)
                            {
                                // Lógica para cancelar la invitación
                                string msgEnvio = "16/" + textUsuario.Text;
                                // Enviamos al servidor la consulta para la DB
                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(msgEnvio);
                                server.Send(msg);
                            }
                        }
                        else if (mensaje == "NoConectado")
                            MessageBox.Show("No te has podido unirte, sala llena");
                        break;
                    case 10://missatge de notificacio
                        MessageBox.Show(messageReceived);
                        MessageBox.Show("El usuario ha aceptado la invitación");
                        break;
                    case 13:
                        MessageBox.Show(messageReceived);
                            string color = trozos[3];
                            int a = Convert.ToInt32(trozos[4]);
                            int num_puntos = Convert.ToInt32(trozos[5]);
                            int k = 0;
                            puntos_color = new List<Point>();
                            if (true) {
                            while (k<= (num_puntos - 1))
                            {
                                string punto = trozos[k+6];
                                string[] coordenadas = punto.Split('-');
                                Point p = new Point(Convert.ToInt32(coordenadas[0]), Convert.ToInt32(coordenadas[1]));
                                puntos_color.Add(p);
                                k++;
                            }
                                
                        }
                            recibir_dibujo(puntos_color, color, a);
                        break;
                    case 15:
                        MessageBox.Show(messageReceived);
                        ganador = trozos[1];
                        info.Text = (ganador + "gana la partida");
                        break;
                    case 16:
                        MessageBox.Show(messageReceived);
                        dibujante = true;
                        btn_pencil.Enabled = true;
                        btn_eraser.Enabled = true;
                        button3.Enabled = true;
                        break;
                    case 18:
                        MessageBox.Show(messageReceived);
                        palabra = trozos[1];
                        //timer.Enabled = true;

                        if (dibujante == true)
                            info.Text = ("Te toca dibujar, la palabra es: " + palabra);
                        else
                            info.Text = ("Empieza la ronda");
                        break;
                    case 20:
                        MessageBox.Show(messageReceived);
                        string m = "20/" + textUsuario.Text + puntuacion;
                        // Enviamos al servidor la consulta para la DB
                        byte[] ms = System.Text.Encoding.ASCII.GetBytes(m);
                        server.Send(ms);
                        break;
                    }
                }
            }
        }

        private void conectar_Click(object sender, EventArgs e)
        {
            IPAddress direc = IPAddress.Parse("192.168.56.102"); //INDICAMOS IP// La ip establecida es un ejemplo
            IPEndPoint ipep = new IPEndPoint(direc, 9100); //INDICAMOS PUERTO// El puerto establecido es un ejemplo

            //Creamos el socket
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ipep); //Intentamos conectar el socket
            }
            catch (SocketException ex)
            {
                //Si hay excepcion indicamos error y salimos del programa con un return
                MessageBox.Show("Conexion con servidor fallida: " + ex.Message);
                return;
            }

            ThreadStart ts = delegate { atenderServidor(); };
            atender = new Thread(ts);
            atender.Start();

            //cambia el texto de los labels para informar al usuario, ademas desactiva o activa botones para evitar errores y que el usuario sepa que puede hacer
            estado.Text = "Conectado";
            desconectar.Enabled = true;
            conectar.Enabled = false;
            iniciar_sesion.Enabled = true;
            registrarse.Enabled = true;
            Consultas_menu.Enabled = true;
        }

        private void desconectar_Click(object sender, EventArgs e)
        {
            //Mensaje de desconexion
            messageSended = "0/"; //codigo 0 para desconectar
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
            MessageBox.Show(messageSended);
            server.Send(msg);

            //Nos desconectamos
            atender.Abort();
            this.BackColor = Color.Gray;
            server.Shutdown(SocketShutdown.Both);
            server.Close();

            //limpiamos la lista, he indicamos al usuario que para poder observar quien esta conectado tiene que inciar sesión
            lista.Items.Clear();
            lista.Items.Add("Inicia sesión para ver quien esta conectado");

            //cambia el texto de los labels para informar al usuario, ademas desactiva o activa botones para evitar errores y que el usuario sepa que puede hacer
            estado.Text = "Desconectado";
            iniciar_sesion.Visible = true;
            registrarse.Visible = true;
            desconectar.Enabled = false;
            conectar.Enabled = true;
            iniciar_sesion.Enabled = false;
            registrarse.Enabled = false;
            Consultas_menu.Enabled = false;
        }

        private void iniciar_sesion_Click(object sender, EventArgs e)
        {
            messageSended = "1/" + textUsuario.Text + "/" + textPassword.Text; /*EL mensaje 1 servira para iniciar sesion, el servidor tendra que
            buscar en la base de datos al usuario y enviar un mensaje de vuelta que ponga que SI esta. */
            MessageBox.Show(messageSended);

            //antes de enviar se comprobara que el usuario ha introducido correctamente los campos necesarios
            if ((textUsuario.Text != "") && (textPassword.Text != ""))
            {
                // Enviamos al servidor el nombre y contraseña de teclado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
                server.Send(msg);
            }
            else
            {
                MessageBox.Show("Introduzca un usuario o contraseña");
            }
        }

        private void registrarse_Click(object sender, EventArgs e)
        {
            messageSended = "2/" + textUsuario.Text + "/" + textPassword.Text; /*EL mensaje 2 servira para registrarse, el servidor tendra que
            insertar en la base de datos al usuario y enviar un mensaje de vuelta que lo verifique. */
            MessageBox.Show(messageSended);
            //antes de enviar se comprobara que el usuario ha introducido correctamente los campos necesarios
            if ((textUsuario.Text != "") && (textPassword.Text != ""))
            {
                // Enviamos al servidor el nombre y contraseña de teclado
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
                server.Send(msg);
            }
            else
            {
                MessageBox.Show("Introduzca un usuario o contraseña");
            }
        }

        private void invitar_Click(object sender, EventArgs e)
        {
            string messageSended = "7/" + textUsuario.Text + "/" + invitacion.Text + "/" + numsala;
            // Enviamos al servidor la consulta para la DB
            MessageBox.Show(messageSended);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
            server.Send(msg);
        }

        private void crearSala_Click(object sender, EventArgs e)
        {
            string messageSended = "6/" + textUsuario.Text;
            // Enviamos al servidor la consulta para la DB
            MessageBox.Show(messageSended);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
            server.Send(msg);
            creador = true;
        }

        private void BtnAbandon_Click(object sender, EventArgs e)
        {
            string mensaje = "11/" + textUsuario.Text + "/" + numsala;
            // Enviamos al servidor la consulta para la DB
            MessageBox.Show(messageSended);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
            server.Send(msg);
        }
        private void start_Click(object sender, EventArgs e)//El creador de la sala empieza la partida
        {
            if (creador == true)
            {
                messageSended = "12/" + textUsuario.Text + "/" + numsala;
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
                MessageBox.Show(messageSended);
                server.Send(msg);
                //await Task.Delay(1000);
                //string mensajex = "18/" + textUsuario.Text + "/" + numsala;
                //byte[] msgx = System.Text.Encoding.ASCII.GetBytes(mensajex);
                //server.Send(msgx);
            }
            else
                MessageBox.Show("Solo el creador puede empezar la partida");
        }
        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Palabra: " + palabra);
            MessageBox.Show("Respuesta: " + respuesta.Text);
            string rp = respuesta.Text;
            if (palabra == respuesta.Text){
                puntuacion = puntuacion + contador;
                info.Text = ("¡Has acertado!");
                label12.Text = Convert.ToString(puntuacion);
            }
            else
                info.Text = ("No es correcta");
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            /*contador--;
            tiempo.Text = Convert.ToString(contador);
            if (contador == 0)
            {
                timer.Enabled = false;
                info.Text = ("¡Se acabó el tiempo");
                MessageBox.Show("La palabra era:" + palabra);
                contador = 60;
                tiempo.Text = Convert.ToString(contador);
                if (dibujante == true)
                {
                    dibujante = false;
                    btn_pencil.Enabled = false;
                    btn_eraser.Enabled = false;
                    button3.Enabled = false;
                }
                if (creador == true)
                {
                    string mensaje = "18/" + textUsuario.Text + "/" + numsala;
                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                    server.Send(msg);
                }
            }*/
        }

        private void recibir_dibujo(List<Point> puntos_color, string cl, int e)
        {
            Color col = Color.FromName(cl);
            pen.Color = col;
            for (int i = 0; i < puntos_color.Count-2; i++)
            {
                g.DrawLine(pen, puntos_color.ElementAt(i), puntos_color.ElementAt(i+1));

            }

            //else
            //    g.DrawLine(erase, px, py);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pic.Image = bm;
            index = 0;

        }

        private void btn_color_Click(object sender, EventArgs e)
        {
            cd.ShowDialog();
            new_color = cd.Color;
            pic_color.BackColor = cd.Color;
            pen.Color = cd.Color;
        }

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            //paint = true;
            //py = e.Location;  
        }

        private void color_picker_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = set_point(color_picker, e.Location);
            pic_color.BackColor = ((Bitmap)color_picker.Image).GetPixel(point.X, point.Y);
            new_color = pic_color.BackColor;
            pen.Color = pic_color.BackColor;
        }

        

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.Location;
            textBoxPuntoX.Text = p.X.ToString();
            textBoxPuntoY.Text = p.Y.ToString();

            //if (paint==true)
            //{
            //    if(index==1)
            //    {
            //        Point p = e.Location;
            //        g.DrawLine(pen,p.X,p.Y);
            //        a = 1;
            //        //Enviar_Dibujo(px,py,p,a);

            //        puntos_color[pos_puntos_color] = px.X;
            //        puntos_color[pos_puntos_color + 1] = px.Y;
            //        pos_puntos_color = pos_puntos_color + 2;
            //        if (pos_puntos_color == 50)
            //        {
            //            int z = 0;
            //            Enviar_puntos(puntos_color, p, a, pos_puntos_color);
            //            while (z < 50)
            //            {
            //                puntos_color[z] = 0;
            //                z++;
            //            }
            //            pos_puntos_color = 0;

            //        }

            //        py = px;
            //    }
            //    if (index == 2)
            //    {
            //        px = e.Location;
            //        a = 50;
            //        g.DrawLine(erase, px, py);
            //        puntos_borrar[pos_puntos_borrar] = px.X;
            //        puntos_borrar[pos_puntos_borrar + 1] = px.Y;
            //        pos_puntos_borrar = pos_puntos_borrar + 2;
            //        if(pos_puntos_borrar == 50)
            //        {
            //            int z = 0;
            //            Enviar_puntos(puntos_borrar, erase, a, pos_puntos_borrar);
            //            while(z < 50)
            //            {
            //                puntos_borrar[z] = 0;
            //                z++;
            //            }
            //            pos_puntos_borrar = 0;

            //        }


            //        //Enviar_Dibujo(px, py, erase,a);
            //        py = px;
            //    }
            //}
            pic.Refresh();
        }
        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            // paint = false;
            
            //if (index == 1 || index == 50) {
                
            //    if (a == 1)
            //    {
            //        int z = 0;
            //        Enviar_puntos(puntos_color, p, a, pos_puntos_color);
            //        while (z < 50)
            //        {
            //            puntos_color[z] = 0;
            //            z++;
            //        }
            //        pos_puntos_color = 0;
            //    }
            //    else
            //    {
            //        int z = 0;
            //        Enviar_puntos(puntos_borrar, erase, a, pos_puntos_borrar);
            //        while (z < 50)
            //        {
            //            puntos_borrar[z] = 0;
            //            z++;
            //        }
            //        pos_puntos_borrar = 0;
            //    }
            //}
        }

        private void btn_eraser_Click(object sender, EventArgs e)
        {
            index = 2;
        }

        private void btn_pencil_Click(object sender, EventArgs e)
        {
            index = 1;
        }
        private void pic_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < puntos_color.Count - 2; i++)
            {
                g.DrawLine(pen, puntos_color.ElementAt(i), puntos_color.ElementAt(i + 1));
            }
            this.pic.Refresh();
           
        }

        private void comoSeJuegaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("¡Bienvenidos a pinturillo! Este juego consiste en adivinar que objeto o idea está dibujando el rival.\r\n\r\n¿Como funciona? En cada partida hay un dibujante y los otros han de adivinar tu dibujo.\r\n\r\nSi te ha tocado dibujar: Tienes 60 segundos para dibujar la palabra que te ha tocado aleatoriamente, ten en cuenta que si nadie adivina la palabra, se va a restar tu puntuacíon, así que procura que se pueda adivinar facilmente.\r\n\r\nSi tienes que adivinar: Tienes 60 segundos para escribir la palabra correcta que tu contrincante está dibujando, cuando antes lo adivines, más puntos te llevas.\r\n\r\nCrea o únete a una sala y cuando estéis todos listos, que el creador de la sala le de al botón para empezar.\r\n\r\n¡Mucha suerte!\r\n\r\n \r\n");
        }

        private void queEsCadaConsultaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Consulta 1: Jugador que ha conseguido la puntuación más alta en una partida. \r\n\r\nConsulta 2: Jugador con la puntuación más baja. \r\n\r\nConsulta 3: Jugador que ha jugado más partidas");
        }

        private void consulta1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            messageSended = "3/";
            // Enviamos al servidor la consulta para la DB
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
            MessageBox.Show(messageSended);
            server.Send(msg);
        }

        private void consulta2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            messageSended = "4/";
            // Enviamos al servidor la consulta para la DB
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
            MessageBox.Show(messageSended);
            server.Send(msg);
        }

        private void consulta3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            messageSended = "5/";
            // Enviamos al servidor la consulta para la DB
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
            MessageBox.Show(messageSended);
            server.Send(msg);
        }
       

        static Point set_point(PictureBox pb, Point pt)
        {
            float px = 1f*pb.Image.Width/ pb.Width;
            float py = 1f*pb.Image.Height/ pb.Height;
            return new Point((int)(pt.X*px), (int)(pt.Y*py));
        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            puntos_color.Add(new Point(e.X, e.Y));
            if(puntos_color.Count > 2)
                g.DrawLine(pen, puntos_color.ElementAt(puntos_color.Count-2), puntos_color.ElementAt(puntos_color.Count - 1));
        }

        private void btn_Send_Click(object sender, EventArgs e)
        {
            Enviar_puntos(puntos_color, pen, 1);
        }

        private void Enviar_puntos(List<Point> p, Pen pen, int a)
        {
           if(a == 1)
           {
                string puntos_color = "/";
                int f = 0;
                while(f< p.Count)
                {
                    puntos_color += p.ElementAt(f).X + "-" + p.ElementAt(f).Y + "/";
                    f++;
                }
                String color = Convert.ToString(pen.Color);
                messageSended = "13/" + textUsuario.Text + "/" + numsala + "/" + color + "/" + a + "/" + p.Count + puntos_color;
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(messageSended);
                MessageBox.Show(messageSended);
                server.Send(msg);
           }
           else
           {
                //string Puntos_borrar = null;
                //int f = 0;
                //while (f < pos_puntos_color)
                //{
                //    string punto = Convert.ToString(p[f]);
                //    Puntos_borrar += "/" + punto;
                //    f++;
                //}
                //String color = Convert.ToString(pen.Color);
                //string mensaje = "13/" + textUsuario.Text + "/" + numsala + "/" + color + "/" + a + "/" + pos_puntos_borrar + Puntos_borrar ;
                //byte[] msg = System.Text.Encoding.ASCII.GetBytes(mensaje);
                //server.Send(msg);
           }
        }
    }
}