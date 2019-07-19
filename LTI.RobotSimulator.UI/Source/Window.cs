﻿using LTI.RobotSimulator.Core;
using SFML.Graphics;
using SFML.Window;
using System;
using System.Drawing;
using System.Windows.Forms;
using Color = SFML.Graphics.Color;
using Sensor = LTI.RobotSimulator.Core.Sensor;

namespace LTI.RobotSimulator.UI
{
    public partial class Window : Form
    {
        public Window()
        {
            InitializeComponent();
        }

        private RenderWindow Surface { get; set; }

        private void Window_Load(object sender, EventArgs e)
        {
            Simulation.Initialize();

            // Reads the settings file and sets the label size (render target)
            SetupFile.Read(@"..\..\..\resource\settings.ini");
            renderTarget.Size = SetupFile.SurfaceSize;

            Simulation.Robot.X = renderTarget.Width / 2;
            Simulation.Robot.Y = renderTarget.Height / 2;

            ClientSize = new Size(renderTarget.Location.X + renderTarget.Width + 6 + robotGroupBox.Width + 12, renderTarget.Location.Y + renderTarget.Height + 12);
            robotGroupBox.Location = new System.Drawing.Point(renderTarget.Location.X + renderTarget.Width + 6, 24);
            robotFileGroupBox.Location = new System.Drawing.Point(renderTarget.Location.X + renderTarget.Width + 6, 472);

            CenterToScreen();

            Surface = new RenderWindow(renderTarget.Handle, new ContextSettings());
            Surface.SetFramerateLimit(60);
        }

        public void OnUpdate()
        {
            if (Simulation.HasStarted)
            {
                if (Simulation.Robot.CanMove)
                {
                    Simulation.Robot.Move();
                    rotationNumericUpDown.Value = -(decimal)Simulation.Robot.Theta;
                }
            }
            else
            {
                Simulation.Robot.Theta = -(float)rotationNumericUpDown.Value;
            }

            Simulation.Robot.Speed = (float)speedNumericUpDown.Value;
            Simulation.Robot.LeftWheel.Text.DisplayedString = Simulation.Robot.LeftWheel.Speed.ToString();
            Simulation.Robot.RightWheel.Text.DisplayedString = Simulation.Robot.RightWheel.Speed.ToString();
        }

        public void OnRender()
        {
            Surface.Clear(new Color(100, 150, 200, 255));

            // Draws obstacles
            foreach (DrawableLine obstacle in Simulation.Obstacles)
            {
                obstacle.Draw(Surface, RenderStates.Default);
            }

            // Draws the trajectory and the point cloud from a robot file
            if (RobotFile.Loaded)
            {
                if (robotFileTrajectoryCheckBox.Checked)
                {
                    foreach (TrajectoryPoint trajectoryPoint in RobotFile.Trajectory.Points)
                    {
                        trajectoryPoint.Draw(Surface, RenderStates.Default);
                    }
                }

                if (robotFilePointCloudCheckBox.Checked)
                {
                    foreach (CloudPoint cloudPoint in RobotFile.PointCloud.Points)
                    {
                        cloudPoint.Draw(Surface, RenderStates.Default);
                    }
                }
            }

            // Draws the robot's trajectory
            if (robotTrajectoryCheckBox.Checked)
            {
                foreach (TrajectoryPoint trajectoryPoint in Simulation.Robot.Trajectory.Points)
                {
                    trajectoryPoint.Draw(Surface, RenderStates.Default);
                }
            }

            // Draws the robot's point cloud
            if (robotPointCloudCheckBox.Checked)
            {
                foreach (CloudPoint cloudPoint in Simulation.Robot.PointCloud.Points)
                {
                    cloudPoint.Draw(Surface, RenderStates.Default);
                }
            }

            // Draws the robot
            Simulation.Robot.Draw(Surface, RenderStates.Default);

            // Drawsthe robot's sensors
            if (sensorsCheckBox.Checked)
            {
                foreach (Sensor sensor in Simulation.Robot.Sensors)
                {
                    sensor.Draw(Surface, RenderStates.Default);
                }
            }

            // Draws the wheel's text
            if (wheelsCheckBox.Checked)
            {
                Simulation.Robot.LeftWheel.Text.Draw(Surface, RenderStates.Default);
                Simulation.Robot.RightWheel.Text.Draw(Surface, RenderStates.Default);
            }

            Surface.Display();
        }

        #region Event functions
        private void RenderTarget_MouseClick(object sender, MouseEventArgs e)
        {
            // Moves the robot with the mouse and lets the user to define a path from 2 points
            if (robotStartButton.Enabled)
            {
                if (e.Button == MouseButtons.Left) // Moves the robot with the mouse and defines a start point
                {
                    Simulation.Robot.X = e.Location.X;
                    Simulation.Robot.Y = e.Location.Y;
                }
                else if (e.Button == MouseButtons.Right) // Defines an end point
                {
                    Simulation.Robot.EndPoint.X = e.Location.X;
                    Simulation.Robot.EndPoint.Y = e.Location.Y;
                }
            }
        }

        private void ResetSpeedButton_Click(object sender, EventArgs e)
        {
            speedNumericUpDown.Value = 0.0M;
        }

        private void ResetRotationButton_Click(object sender, EventArgs e)
        {
            rotationNumericUpDown.Value = 0;
        }

        private void RobotStartButton_Click(object sender, EventArgs e)
        {
            if (Simulation.Robot.DefinedPath)
            {
                Simulation.HasStarted = true;
                Simulation.Robot.EnableMovement();

                resetSpeedButton.Enabled = false;
                resetRotationButton.Enabled = false;
                robotStartButton.Enabled = false;
                robotPauseButton.Enabled = true;
                rotationNumericUpDown.Enabled = false;
            }
            else
            {
                MessageBox.Show("The robot path isn't defined yet!", "Error");
            }
        }

        private void RobotPauseButton_Click(object sender, EventArgs e)
        {
            Simulation.Robot.DisableMovement();

            robotPauseButton.Enabled = false;
            robotResumeButton.Enabled = true;
        }

        private void RobotResumeButton_Click(object sender, EventArgs e)
        {
            Simulation.Robot.EnableMovement();

            robotPauseButton.Enabled = true;
            robotResumeButton.Enabled = false;
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (saveFileDialog)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    RobotFile.Save(saveFileDialog.FileName);
                }
            }
        }

        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (openFileDialog)
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    RobotFile.Load(openFileDialog.FileName);
                    robotFileGroupBox.Text = openFileDialog.SafeFileName;
                    robotFileGroupBox.Visible = true;
                }
            }
        }
        #endregion
    }
}