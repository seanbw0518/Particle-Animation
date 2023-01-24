using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace Particles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer aTimer;
        
        Random rng = new Random();

        bool paused = true;

        bool particlesDrawn = false;

        Config config = new Config();

        // all particle types
        private List<ParticleType> particleTypes = new List<ParticleType>{
            new ParticleType(1, 1, new int[]{255,0,0}, 100, 4),
            new ParticleType(1, 1, new int[]{0,255,0}, 100, 4),
            new ParticleType(1, 1, new int[]{0,0,255}, 100, 4),
            //new ParticleType(2, 1, 4, new int[]{0,255,255}, 100, 4),
            //new ParticleType(-1, 2, 10, new int[]{255,0,255}, 100, 4),
        };

        public MainWindow()
        {
            InitializeComponent();

            config = ReadConfigFromXml();
            InitializeSliders();
            InitializeParticleTypes();

            aTimer = new System.Windows.Threading.DispatcherTimer();
            aTimer.Tick += new EventHandler(Draw);
            aTimer.Interval = TimeSpan.FromMilliseconds(config.framerate);
        }

        // to store all the particles
        private List<Particle> particles = new List<Particle>();

        private void WriteConfigToXml()
        {

            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(Config));

            var path = "../Config.xml";
            System.IO.FileStream file = System.IO.File.Create(path);

            writer.Serialize(file, config);
            file.Close();

        }

        private Config ReadConfigFromXml()
        {
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(Config));
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader("../Config.xml");
                Config config = (Config)reader.Deserialize(file);
                file.Close();
                return config;

            }
            catch (Exception)
            {
                return config;
            }

        }

        //private void WriteParticleTypesToXml(object sender, RoutedEventArgs e)
        //{

        //    System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(Config));

        //    var path = "../ParticleTypes.xml";
        //    System.IO.FileStream file = System.IO.File.Create(path);

        //    writer.Serialize(file, particleTypes);
        //    file.Close();

        //}

        //private List<ParticleType> ReadParticleTypesFromXml()
        //{
        //    System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(List<ParticleType>));
        //    try
        //    {
        //        System.IO.StreamReader file = new System.IO.StreamReader("../ParticleTypes.xml");
        //        List<ParticleType> particleTypes = (List<ParticleType>)reader.Deserialize(file);
        //        file.Close();
        //        return particleTypes;

        //    }
        //    catch (Exception)
        //    {
        //        return particleTypes;
        //    }

        //}

        private void Draw(object source, EventArgs e)
        {

            Canvas canvas = (Canvas)this.FindName("Canvas");

            particles = Animate(particles, canvas);
            canvas.Children.Clear();


            foreach (Particle particle in particles)
            {

                Ellipse particleE = new Ellipse();
                particleE.Height = particle.type.radius;
                particleE.Width = particle.type.radius;
                particleE.Fill = new SolidColorBrush(particle.type.color);
                Canvas.SetLeft(particleE, particle.x);
                Canvas.SetTop(particleE, particle.y);

                //debug
                //TextBlock text = new TextBlock();
                //text.Text =""+ (int) particle.vy;
                //text.Foreground = new SolidColorBrush(Colors.White);
                //Canvas.SetLeft(text, particle.x);
                //Canvas.SetTop(text, particle.y);

                //canvas.Children.Add(text);
                canvas.Children.Add(particleE);
                


            }

        }

        private List<Particle> Animate(List<Particle> particles, Canvas canvas)
        {
            // state of particles before calculation
            Particle[] initialState = particles.ToArray();

            foreach (Particle particle0 in initialState)
            {
                // start by setting forces to zero
                float fx = 0;
                float fy = 0;
                foreach (Particle particle1 in initialState)
                {

                    // skip itself
                    if (particle1 == particle0) continue;

                    // distance in x and y directions
                    float dx = (float)(particle0.x - particle1.x);
                    float dy = (float)(particle0.y - particle1.y);

                    // d = distance between two particles
                    float d = (float)Math.Sqrt((dx * dx) + (dy * dy));

                    // force
                    float f;

                    #region Calculate charge force
                    if (d > config.minKRange && d < config.maxKRange)
                    {
                        f = (float)(config.k * (particle0.type.charge * particle1.type.charge) / (d * d));

                        fx += f * dx;
                        fy += f * dy;
                    }
                    else if (d < config.minKRange)
                    {
                        particle0.vx *= config.kLoss;
                        particle0.vy *= config.kLoss;
                    }
                    #endregion

                    #region Calculate reaction force
                    if (d > config.minRRange && d < config.maxRRange)
                    {
                        particle0.reactivity = particle0.type.reactivity;

                        f = config.r * (particle0.reactivity * particle1.reactivity / (d * d));

                        fx += f * dx;
                        fy += f * dy;

                    }
                    else if (d < config.minRRange && d > 0)
                    {
                        float dif = particle0.reactivity - particle1.reactivity;

                        particle0.reactivity -= dif;

                        f = -config.r * ((Math.Abs(dif-config.r)) / (d * d));

                        fx += f * dx;
                        fy += f * dy;

                        particle0.vx *= config.rLoss;
                        particle0.vy *= config.rLoss;
                    }
                    else if (d == 0)
                    {
                        particle0.vx *= config.rLoss;
                        particle0.vy *= config.rLoss;
                    }
                    else
                    {
                        particle0.reactivity = particle0.type.reactivity;
                    }
                    #endregion
                }

                // f=ma (m=1)
                particle0.vx += fx;
                particle0.vy += fy;

                // limit velocities
                if (particle0.vy > config.maxV) particle0.vy = config.maxV;
                else if (particle0.vy < -config.maxV) particle0.vy = -config.maxV;
                if (particle0.vx > config.maxV) particle0.vx = config.maxV;
                else if (particle0.vx < -config.maxV) particle0.vx = -config.maxV;

                particle0.x += particle0.vx * config.vMult;
                particle0.y += particle0.vy * config.vMult;

                #region bounce off edges
                if (particle0.x+particle0.type.radius > canvas.ActualWidth)
                {
                    particle0.vx *= -1 * config.bounceLoss;
                    particle0.x = canvas.ActualWidth- particle0.type.radius;
                }
                else if (particle0.x < 0)
                {
                    particle0.vx *= -1 * config.bounceLoss;
                    particle0.x = 0 + particle0.type.radius;
                }
                if (particle0.y+ particle0.type.radius > canvas.ActualHeight)
                {
                    particle0.vy *= -1 * config.bounceLoss;
                    particle0.y = canvas.ActualHeight- particle0.type.radius;
                }
                else if (particle0.y < 0)
                {
                    particle0.vy *= -1 * config.bounceLoss;
                    particle0.y = 0 + particle0.type.radius;
                }
                #endregion

            }
            return particles;
        }

        private void ResetParticles(object sender, RoutedEventArgs e)
        {

            Canvas canvas = (Canvas) this.FindName("Canvas");
            particles.Clear();
            canvas.Children.Clear();
            particlesDrawn = false;

            foreach (ParticleType particleType in particleTypes)
            {
                for (int i = 0; i < particleType.count; i++)
                {

                    Particle temp = new Particle(rng.Next(0, (int)canvas.ActualWidth), rng.Next(0, (int) canvas.ActualHeight), 0, 0, particleType, particleType.reactivity);

                    particles.Add(temp);

                    Ellipse particle = new Ellipse();
                    particle.Height = particleType.radius;
                    particle.Width = particleType.radius;
                    particle.Fill = new SolidColorBrush(particleType.color);
                    Canvas.SetLeft(particle, rng.Next(0, (int)canvas.ActualWidth-5));
                    Canvas.SetTop(particle, rng.Next(5, (int)canvas.ActualHeight-5));
                    canvas.Children.Add(particle);

                }
            }
            particlesDrawn = true;

            WriteConfigToXml();
        }

        private void ResetParticles()
        {

            Canvas canvas = (Canvas)this.FindName("Canvas");
            particles.Clear();
            canvas.Children.Clear();
            particlesDrawn = false;

            foreach (ParticleType particleType in particleTypes)
            {
                for (int i = 0; i < particleType.count; i++)
                {

                    Particle temp = new Particle(rng.Next(0, (int)canvas.ActualWidth), rng.Next(0, (int)canvas.ActualHeight), 0, 0, particleType, particleType.reactivity);

                    particles.Add(temp);

                    Ellipse particle = new Ellipse();
                    particle.Height = particleType.radius;
                    particle.Width = particleType.radius;
                    particle.Fill = new SolidColorBrush(particleType.color);
                    Canvas.SetLeft(particle, rng.Next(0, (int)canvas.ActualWidth - 5));
                    Canvas.SetTop(particle, rng.Next(5, (int)canvas.ActualHeight - 5));
                    canvas.Children.Add(particle);

                }
            }
            particlesDrawn = true;

            WriteConfigToXml();

        }

        private void StartStop(object sender, RoutedEventArgs e)
        {
            Button startStopButton = (Button)this.FindName("StartStopButton");

            if (paused)
            {
                
                startStopButton.Content = "Stop";

                if (!particlesDrawn)
                {
                    ResetParticles();
                    particlesDrawn = true;
                }
                aTimer.Start();
                paused = false;
            }
            else
            {
                startStopButton.Content = "Start";

                aTimer.Stop();
                paused = true;
            }
        }

        private void InitializeParticleTypes()
        {
            StackPanel stack = (StackPanel)this.FindName("TypeStack");

            stack.Children.Clear();

            #region Setup Grid
            RowDefinition[] rows = new RowDefinition[5];
            ColumnDefinition[] columns = new ColumnDefinition[6];

            for (int i = 0; i < particleTypes.Count; i++)
            {
                Grid typeGrid = new Grid();
                typeGrid.Name = "type_" + i;
                typeGrid.Margin = new Thickness(5, 0, 5, 0);

                // Draw Rows.
                for (int r = 0; r < 5; r++)
                {
                    rows[r] = new RowDefinition();
                    typeGrid.RowDefinitions.Add(rows[r]);

                    // Setting Row height.
                    if (r == 0 || r == 4)
                    {
                        rows[r].Height = new GridLength(35);
                    }
                    else
                    {
                        rows[r].Height = new GridLength(25);
                    }
                }
                // Draw Columns.
                for (int c = 0; c < 6; c++)
                {
                    columns[c] = new ColumnDefinition();
                    typeGrid.ColumnDefinitions.Add(columns[c]);

                    // Setting column width.
                    if (c % 2 == 0)
                    {
                        columns[c].Width = new GridLength(20);
                    }
                    else
                    {
                        columns[c].Width = new GridLength(35);
                    }
                }
                #endregion

                // set title, e.g. "Type 0"
                Label title = new Label();
                title.Content = "Type " + i;
                title.HorizontalContentAlignment = HorizontalAlignment.Center;
                title.VerticalAlignment = VerticalAlignment.Center;
                title.VerticalContentAlignment = VerticalAlignment.Stretch;
                title.FontWeight = FontWeights.Bold;
                title.FontSize = 14;
                Grid.SetRow(title, 0);
                Grid.SetColumn(title, 0);
                Grid.SetColumnSpan(title, 6);
                typeGrid.Children.Add(title);

                // set Q
                Label Q = new Label();
                Q.Content = "Q";
                Q.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetRow(Q, 1);
                Grid.SetRowSpan(Q, 1);
                Grid.SetColumn(Q, 0);
                Grid.SetColumnSpan(Q, 1);
                typeGrid.Children.Add(Q);

                TextBox qBox = new TextBox();
                qBox.Name = "qBox_" + i;
                //RegisterName(qBox.Name, qBox);
                qBox.KeyDown += QChanged;
                qBox.Text = particleTypes.ElementAt(i).charge + "";
                qBox.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(qBox, 1);
                Grid.SetRowSpan(qBox, 1);
                Grid.SetColumn(qBox, 1);
                Grid.SetColumnSpan(qBox, 1);
                typeGrid.Children.Add(qBox);

                // randomize button
                Button randomizeButton = new Button();
                randomizeButton.Content = "*";
                randomizeButton.Name = "randomizeButton_"+i;
                randomizeButton.Margin = new Thickness(5, 0, 5, 0);
                randomizeButton.Padding = new Thickness(5, 0, 5, 0);
                randomizeButton.VerticalAlignment = VerticalAlignment.Center;
                randomizeButton.Click += RandomizeType;
                Grid.SetRow(randomizeButton, 1);
                Grid.SetRowSpan(randomizeButton, 1);
                Grid.SetColumn(randomizeButton, 2);
                Grid.SetColumnSpan(randomizeButton, 2);
                typeGrid.Children.Add(randomizeButton);

                // set R
                Label R = new Label();
                R.Content = "R";
                R.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetRow(R, 1);
                Grid.SetRowSpan(R, 1);
                Grid.SetColumn(R, 4);
                Grid.SetColumnSpan(R, 1);
                typeGrid.Children.Add(R);

                TextBox rBox = new TextBox();
                rBox.Name = "rBox_" + i;
                //RegisterName(rBox.Name, rBox);
                rBox.KeyDown += RChanged;
                rBox.Text = particleTypes.ElementAt(i).reactivity + "";
                rBox.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(rBox, 1);
                Grid.SetRowSpan(rBox, 1);
                Grid.SetColumn(rBox, 5);
                Grid.SetColumnSpan(rBox, 1);
                typeGrid.Children.Add(rBox);

                // set #
                Label count = new Label();
                count.Content = "#";
                count.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetRow(count, 2);
                Grid.SetRowSpan(count, 1);
                Grid.SetColumn(count, 0);
                Grid.SetColumnSpan(count, 1);
                typeGrid.Children.Add(count);

                TextBox countBox = new TextBox();
                countBox.Name = "countBox_" + i;
                countBox.KeyDown += CountChanged;
                countBox.Text = particleTypes.ElementAt(i).count + "";
                countBox.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(countBox, 2);
                Grid.SetRowSpan(countBox, 1);
                Grid.SetColumn(countBox, 1);
                Grid.SetColumnSpan(countBox, 1);
                typeGrid.Children.Add(countBox);

                // set radius
                Label radius = new Label();
                radius.Content = "Radius";
                radius.HorizontalAlignment = HorizontalAlignment.Left;
                Grid.SetRow(radius, 2);
                Grid.SetRowSpan(radius, 1);
                Grid.SetColumn(radius, 3);
                Grid.SetColumnSpan(radius, 2);
                typeGrid.Children.Add(radius);

                TextBox radiusBox = new TextBox();
                radiusBox.Name = "radiusBox_" + i;
                radiusBox.KeyDown += RadiusChanged;
                radiusBox.Text = particleTypes.ElementAt(i).radius + "";
                radiusBox.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(radiusBox, 2);
                Grid.SetRowSpan(radiusBox, 1);
                Grid.SetColumn(radiusBox, 5);
                Grid.SetColumnSpan(radiusBox, 1);
                typeGrid.Children.Add(radiusBox);

                // set color picker
                ColorPicker colorPicker = new ColorPicker();
                colorPicker.Name = "colorPicker_" + i;
                colorPicker.SelectedColor = particleTypes.ElementAt(i).color;
                colorPicker.SelectedColorChanged += ColorChanged;
                Grid.SetRow(colorPicker, 3);
                Grid.SetRowSpan(colorPicker, 1);
                Grid.SetColumn(colorPicker, 0);
                Grid.SetColumnSpan(colorPicker, 6);
                typeGrid.Children.Add(colorPicker);

                // set remove button
                Button button = new Button();
                button.Name = "remove_" + i;
                button.Content = "Remove";
                button.Margin = new Thickness(0, 5, 0, 0);
                button.Height = 25;
                button.VerticalAlignment = VerticalAlignment.Center;
                button.Click += RemoveParticleType;
                Grid.SetRow(button, 4);
                Grid.SetRowSpan(button, 1);
                Grid.SetColumn(button, 0);
                Grid.SetColumnSpan(button, 6);
                typeGrid.Children.Add(button);

                // add to ui
                stack.Children.Add(typeGrid);
            }
        }

        private void RandomizeType(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string typeToEdit = button.Name;

            int index = Int32.Parse(typeToEdit.Split("_")[1]);

            float newCharge = (float)Math.Round((rng.NextDouble() * (3 - -3) + -3),1);
            float newReactivity = (float)Math.Round((rng.NextDouble() * (3 - -3) + -3),1);

            particleTypes.ElementAt(index).charge = newCharge;
            particleTypes.ElementAt(index).reactivity = newReactivity;

            TextBox rBox = (TextBox)LogicalTreeHelper.FindLogicalNode(button.Parent, "rBox_"+index);
            TextBox qBox = (TextBox)LogicalTreeHelper.FindLogicalNode(button.Parent, "qBox_" + index);

            rBox.Text = ""+newReactivity;
            qBox.Text = ""+newCharge;
        }

        private void QChanged(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Return)
            {
                TextBox textBox = (TextBox)sender;
                string typeToEdit = textBox.Name;

                int index = Int32.Parse(typeToEdit.Split("_")[1]);
                particleTypes.ElementAt(index).charge = Double.Parse(textBox.Text);
            }

        }

        private void RChanged(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Return)
            {
                TextBox textBox = (TextBox)sender;
                string typeToEdit = textBox.Name;

                int index = Int32.Parse(typeToEdit.Split("_")[1]);
                try
                {
                    particleTypes.ElementAt(index).reactivity = float.Parse(textBox.Text);
                }catch (Exception)
                {
                    MessageBox.Show("Invalid Reactivity - must be a number.");
                }
            }

        }

        private void CountChanged(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Return)
            {
                TextBox textBox = (TextBox)sender;
                string typeToEdit = textBox.Name;

                int index = Int32.Parse(typeToEdit.Split("_")[1]);
                particleTypes.ElementAt(index).count = Int32.Parse(textBox.Text);
            }

        }

        private void RadiusChanged(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Return)
            {
                TextBox textBox = (TextBox)sender;
                string typeToEdit = textBox.Name;

                int index = Int32.Parse(typeToEdit.Split("_")[1]);

                try
                {
                    var input = Int32.Parse(textBox.Text);
                    if (input < 1)
                    {
                        MessageBox.Show("Invalid Radius - must be > 0.");
                    }
                    else
                    {
                        particleTypes.ElementAt(index).radius = Int32.Parse(textBox.Text);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid Radius - must be an integer.");
                }
            }
        }

        private void ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            ColorPicker picker = (ColorPicker)sender;
            string typeToEdit = picker.Name;

            int index = Int32.Parse(typeToEdit.Split("_")[1]);
            particleTypes.ElementAt(index).color = (Color)picker.SelectedColor;
        }

        private void CreateParticleType(object sender, RoutedEventArgs e)
        {
            particleTypes.Add(new ParticleType(0,0, new int[] { rng.Next(0,255), rng.Next(0, 255), rng.Next(0, 255) }, 0, 4));
            InitializeParticleTypes();
            // create UI with all zeros basically
        }

        private void RemoveParticleType(object sender, RoutedEventArgs e)
        {

            //StackPanel stack = (StackPanel)this.FindName("TypeStack");

            Button button = (Button)sender;
            string typeToRemove = button.Name;

            int index = Int32.Parse(typeToRemove.Split("_")[1]);
            particleTypes.RemoveAt(index);

            //foreach (Particle particle in particles)
            //{
            //    if (particle.type == particleTypes.ElementAt(index))
            //    {
            //        particles.Remove(particle);
            //    }
            //}

            InitializeParticleTypes();
            //stack.Children.RemoveAt(index);
        }

        private void ClearParticles(object sender, RoutedEventArgs e)
        {
            Canvas canvas = (Canvas)this.FindName("Canvas");
            particles.Clear();
            canvas.Children.Clear();
            particlesDrawn = false;
        }

        private void InitializeSliders()
        {
            Slider slider;

            slider = (Slider)this.FindName("framerateSlider");
            slider.Value = config.framerate;

            slider = (Slider)this.FindName("vMultSlider");
            slider.Value = config.vMult;

            slider = (Slider)this.FindName("kSlider");
            slider.Value = config.k;

            slider = (Slider)this.FindName("rSlider");
            slider.Value = config.r;

            slider = (Slider)this.FindName("maxKRangeSlider");
            slider.Value = config.maxKRange;

            slider = (Slider)this.FindName("minKRangeSlider");
            slider.Value = config.minKRange;

            slider = (Slider)this.FindName("maxRRangeSlider");
            slider.Value = config.maxRRange;

            slider = (Slider)this.FindName("minRRangeSlider");
            slider.Value = config.minRRange;

            slider = (Slider)this.FindName("kLossSlider");
            slider.Value = config.kLoss;

            slider = (Slider)this.FindName("rLossSlider");
            slider.Value = config.rLoss;

            slider = (Slider)this.FindName("bounceLossSlider");
            slider.Value = config.bounceLoss;

            slider = (Slider)this.FindName("maxVSlider");
            slider.Value = config.maxV;
        }

        #region Slider Listeners
        private void framerateChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (aTimer != null)
            {
                Slider slider = (Slider)this.FindName("framerateSlider");
                aTimer.Interval = TimeSpan.FromMilliseconds(slider.Value);
            }
            
        }

        private void vMultChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("vMultSlider");
            config.vMult = (float)slider.Value;
        }

        private void kChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("kSlider");
            config.k = (float)slider.Value;
        }

        private void rChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("rSlider");
            config.r = (float)slider.Value;
        }

        private void maxKChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("maxKRangeSlider");
            config.maxKRange = (int)slider.Value;
        }

        private void minKChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("minKRangeSlider");
            config.minKRange = (int)slider.Value;
        }

        private void maxRChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("maxRRangeSlider");
            config.maxRRange = (int)slider.Value;
        }

        private void minRChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("minRRangeSlider");
            config.minRRange = (int)slider.Value;
        }

        private void kLossChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("kLossSlider");
            config.kLoss = slider.Value;
        }

        private void rLossChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("rLossSlider");
            config.rLoss = slider.Value;
        }

        private void bounceLossChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("bounceLossSlider");
            config.bounceLoss = (float)slider.Value;
        }

        private void maxVChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)this.FindName("maxVSlider");
            config.maxV = (float)slider.Value;
        }




        private void framerateChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (aTimer != null)
                {
                    TextBox box = (TextBox)this.FindName("framerateBox");
                    aTimer.Interval = TimeSpan.FromMilliseconds(Int32.Parse(box.Text));
                }
            }

        }

        private void vMultChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("vMultBox");
                config.vMult = float.Parse(box.Text);
            }
            
        }

        private void kChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("kBox");
                config.k = float.Parse(box.Text);
            }
        }

        private void rChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("rBox");
                config.r = float.Parse(box.Text);
            }
        }

        private void maxKChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("maxKRangeBox");
                config.maxKRange = Int32.Parse(box.Text);
            }
        }

        private void minKChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("minKRangeBox");
                config.minKRange = Int32.Parse(box.Text);
            }
        }

        private void maxRChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("maxRRangeBox");
                try
                {
                    config.maxRRange = Int32.Parse(box.Text);
                }catch (Exception)
                {
                    MessageBox.Show("Invalid R. Must be a number > 0");
                }
            }
        }

        private void minRChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("minRRangeBox");
                config.minRRange = Int32.Parse(box.Text);
            }
        }

        private void kLossChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("kLossBox");
                config.kLoss = Double.Parse(box.Text);
            }
        }

        private void rLossChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("rLossBox");
                config.rLoss = Double.Parse(box.Text);
            }
        }

        private void bounceLossChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("bounceLossBox");
                config.bounceLoss = float.Parse(box.Text);
            }
        }

        private void maxVChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TextBox box = (TextBox)this.FindName("maxVBox");
                config.maxV = float.Parse(box.Text);
            }
        }



        #endregion
    }
}
