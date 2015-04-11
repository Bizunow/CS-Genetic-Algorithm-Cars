using System;
using System.Collections.Generic;

using FarseerPhysics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using FarseerPhysics.DebugView;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//using Math;

/// <summary>
///  ласс упрощающий разработку приложени€
/// </summary>
public class Utils
{
    public double lengthdir_x(double len, double degrees)
    {
        double angle = Math.PI * degrees / 180.0;
        return len * Math.Cos(angle);
    }

    public double lengthdir_y(double len, double degrees)
    {
        double angle = Math.PI * degrees / 180.0;
        return len * Math.Sin(angle);
    }
}

namespace Genom
{
    public class genChassis
    {
        public float density = 0.9f;
        public List<float> vertices;

        private const int chassisVertices = 8;
        private const float chassisMaxLength = 2.0f;
        private const float chassisMinLength = 0.4f;

        private Random random;

        public genChassis(Random random)
        {
            this.random = random;
            
            vertices = new List<float>();

            for (int i = 0; i < chassisVertices; i++)
            {
                vertices.Add(Math.Max(Math.Min(random.Next(0, 2), chassisMaxLength), chassisMinLength));
            }
        }

        public void mutate()
        {
            int index = random.Next(0, chassisVertices);
            vertices[index] += ((float)random.Next(-1, 2)) / 20; //5
            vertices[index] = Math.Max(Math.Min(vertices[index], chassisMaxLength), chassisMinLength);
        }

        public genChassis Clone()
        {
            genChassis cloned_gen = new genChassis(random);
            cloned_gen.vertices = new List<float>(this.vertices);
            return cloned_gen;
        }

        public string ToString()
        {
            string info = "Chassis" + Environment.NewLine;

            for (int i = 0; i < 8; i++)
            {
                info += "Vertex " + i.ToString() + ": length = " + vertices[i].ToString() + Environment.NewLine;
            }

            return info;
        }
    }

    public class genWheel
    {
        public int vertex;
        public float radius;
        public float density;

        private const float wheel_radius_max = 0.5f;
        private const float wheel_radius_min = 0.2f;

        private const float wheel_density_max = 1.0f;
        private const float wheel_density_min = 0.2f;

        private const int chassisVertices = 8;

        private Random random;

        public genWheel(Random random)
        {
            this.random = random;

            vertex  = random.Next(0, chassisVertices);
            radius  = 0.3f;
            density = 0.9f;
        }

        public void mutate()
        {
            vertex += random.Next(-1, 2);
            if (vertex < 0) vertex = 7;
            if (vertex > 7) vertex = 0;

            radius = radius + ((float)(random.NextDouble() - 0.5d)) / 20f;
            if (radius > wheel_radius_max) radius = wheel_radius_max;
            if (radius < wheel_radius_min) radius = wheel_radius_min;

            density = density + ((float)(random.NextDouble() - 0.5d)) / 20f;
            if (density > wheel_density_max) density = wheel_density_max;
            if (density < wheel_density_min) density = wheel_density_min;
        }

        public genWheel Clone()
        {
            genWheel cloned_gen = new genWheel(random);
            cloned_gen.vertex = this.vertex;
            cloned_gen.radius = this.radius;
            cloned_gen.density = this.density;
            return cloned_gen;
        }

        public string ToString()
        {
            string info = "Wheel" + Environment.NewLine;
            info += "Vertex: " + vertex.ToString() + Environment.NewLine;
            info += "Radius: " + radius.ToString() + Environment.NewLine;
            info += "Density: " + density.ToString() + Environment.NewLine;
            return info;
        }
    }

    public class Genom
    {
        public genChassis chassis;
        public genWheel wheel_1;
        public genWheel wheel_2;

        private Random random;

        public Genom(Random random)
        {
            this.random = random;

            chassis = new genChassis(random);
            wheel_1 = new genWheel(random);
            wheel_2 = new genWheel(random);
        }

        public void mutate()
        {
            chassis.mutate();
            wheel_1.mutate();
            wheel_2.mutate();

            while (wheel_1.vertex == wheel_2.vertex)
            {
                wheel_1.mutate();
                wheel_2.mutate();
            }
        }

        public Genom Clone()
        {
            Genom genom_clone = new Genom(random);
            genom_clone.chassis = this.chassis.Clone();
            genom_clone.wheel_1 = this.wheel_1.Clone();
            genom_clone.wheel_2 = this.wheel_2.Clone();
            return genom_clone;
        }

        public string ToString() 
        {
            string wheel_1_info = wheel_1.ToString() + Environment.NewLine;
            string wheel_2_info = wheel_2.ToString() + Environment.NewLine;
            string chassis_info = chassis.ToString() + Environment.NewLine;
            return chassis_info + wheel_1_info + wheel_2_info;
        }
    }
}

namespace PhysicalWorld
{
    /// <summary>
    ///  ласс колесо машины
    /// </summary>
    public class Wheel
    {
        private const float restitution = 0.3f;
        private const float friction = 1f;

        private int vertex;
        private float radius;
        private float density;

        public Body body;

        public Wheel(World world, int vertex, float radius, float density)
        {
            this.vertex = vertex;
            this.radius = radius;
            this.density = density;

            CircleShape circle = new CircleShape(radius, density);

            body = new Body(world);
            body.BodyType = BodyType.Dynamic;
            body.CreateFixture(circle);
            body.Friction = friction;
            body.Restitution = restitution;
        }
    }

    /// <summary>
    ///  ласс кузов машины
    /// </summary>
    public class Chassis
    {
        List<float> vertexLengths;
        public Body body;

        public Chassis(World world, List<float> chassisVertexLengths, float density)
        {
            this.vertexLengths = chassisVertexLengths;

            int vertexCount = this.vertexLengths.Count;
            int deegresPerVertex = 360 / vertexCount;

            Utils utils = new Utils();
            Vertices vertices = new Vertices();

            for (int i = 0; i < vertexCount; i++)
            {
                vertices.Add(new Vector2(
                    (float)utils.lengthdir_x(this.vertexLengths[i], i * deegresPerVertex), // meters
                    (float)utils.lengthdir_y(this.vertexLengths[i], i * deegresPerVertex)  // meters
                ));
            }

            PolygonShape chassisShape = new PolygonShape(vertices, density);

            body = new Body(world);
            body.BodyType = BodyType.Dynamic;
            body.Mass = 0.1f;
            body.CreateFixture(chassisShape);
        }
    }

    /// <summary>
    ///  ласс описывающий машину (организм)
    /// </summary>
    public class Car
    {
        private Chassis chassis;
        private Wheel wheel_1;
        private Wheel wheel_2;

        private float max_x;
        private float timer;

        public Genom.Genom genom;

        public Car(World world, Genom.Genom genom)
        {
            chassis = new Chassis(world, genom.chassis.vertices, genom.chassis.density);
            chassis.body.Position = new Vector2(2.5f, -1.0f);
            chassis.body.Mass = 2f;
            chassis.body.Friction = 10f;
            
            max_x = chassis.body.Position.X;

            Utils util = new Utils();

            wheel_1 = new Wheel(world, genom.wheel_1.vertex, genom.wheel_1.radius, genom.wheel_1.density);
            wheel_1.body.Mass = 0.5f;

            wheel_1.body.Position = new Vector2(
                (float)util.lengthdir_x(genom.chassis.vertices[genom.wheel_1.vertex], genom.wheel_1.vertex * 45),
                (float)util.lengthdir_y(genom.chassis.vertices[genom.wheel_1.vertex], genom.wheel_1.vertex * 45)
            ) + chassis.body.Position;

            wheel_2 = new Wheel(world, genom.wheel_2.vertex, genom.wheel_2.radius, genom.wheel_2.density);
            wheel_2.body.Mass = 0.5f;

            wheel_2.body.Position = new Vector2(
                (float)util.lengthdir_x(genom.chassis.vertices[genom.wheel_2.vertex], genom.wheel_2.vertex * 45),
                (float)util.lengthdir_y(genom.chassis.vertices[genom.wheel_2.vertex], genom.wheel_2.vertex * 45)
            ) + chassis.body.Position;
            
            Vector2 axis = new Vector2(0.0f, 1.0f);

            WheelJoint spring_1;
            spring_1 = new WheelJoint(chassis.body, wheel_1.body, wheel_1.body.Position, axis, true);
            spring_1.MotorSpeed = 20f;
            spring_1.MaxMotorTorque = 50f;
            spring_1.MotorEnabled = true;
            spring_1.Frequency = 10f;
            spring_1.DampingRatio = 1f;
            world.AddJoint(spring_1);

            WheelJoint spring_2;
            spring_2 = new WheelJoint(chassis.body, wheel_2.body, wheel_2.body.Position, axis, true);
            spring_2.MotorSpeed = 20.0f;
            spring_2.MaxMotorTorque = 50f;
            spring_2.MotorEnabled = true;
            spring_2.Frequency = 10f;
            spring_2.DampingRatio = 1f;
            world.AddJoint(spring_2);

            this.genom = genom;
        }

        public Vector2 getPosition()
        {
            return chassis.body.Position;
        }

        public void die(World world)
        {
            world.RemoveBody(chassis.body);
            world.RemoveBody(wheel_1.body);
            world.RemoveBody(wheel_2.body);
        }

        public bool isMove(GameTime gameTime)
        {
            float cur_x = chassis.body.Position.X;

            if (cur_x > max_x)
            {
                max_x = cur_x;
                timer = 0f;
            }
            else
            {
                timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            return timer < 3f;
        }

        public float getScore()
        {
            return max_x;
        }
    }

    /// <summary>
    ///  ласс описывающий поверхность
    /// </summary>
    public class Ground
    {
        private Body ground;

        private const int groundLength = 200;
        private const float oneEdgeLength = 0.5f;
        private Vector2 startPos = new Vector2(0, 5);

        public Ground(World world)
        {
            ground = BodyFactory.CreateEdge(world, startPos, startPos += new Vector2(oneEdgeLength, 0));

            Random random = new Random();

            for (int i = 1; i <= groundLength; i++)
            {
                Vector2 randomVector = startPos + new Vector2(oneEdgeLength, (float)(random.NextDouble() - 0.5d) * ((float)i / (groundLength/2)));
                FixtureFactory.AttachEdge(startPos, randomVector, ground);
                startPos = randomVector;
            }

            FixtureFactory.AttachEdge(startPos, new Vector2(startPos.X, startPos.Y - 5), ground);
        }

        public Body getBody()
        {
            return ground;
        }
    }
}

public class Controller
{
    private Random random;

    // –азмер попул€ции
    const int populationSize = 15;

    // √еном попул€ции
    int generation = 0;
    Genom.Genom bestGenom;
    List<float> allGenomsScore;
    List<Genom.Genom> genoms;
    List<float> genomScore;
    int currentGenom = 0;
    float absoluteRecord = 0;
    List<float> allAvgScores;

    // —сылка на машину, если она существует
    public PhysicalWorld.Car currentCar;

    World world;

    public Controller(World world)
    {
        this.world = world;

        random = new Random();

        allGenomsScore = new List<float>();
        allAvgScores = new List<float>();
        genoms = new List<Genom.Genom>();
        genomScore = new List<float>();

        currentCar = null;

        for (int i = 0; i < populationSize; i++)
        {
            Genom.Genom newGenom = new Genom.Genom(random);
            newGenom.mutate();
            genoms.Add(newGenom);
            genomScore.Add(0);
        }
    }

    ~Controller()
    {
        System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\_my\\genom max scores.txt");

        for (int i = 0; i < allGenomsScore.Count; i++)
            file.WriteLine(allGenomsScore[i].ToString());

        file.Close();

        file = new System.IO.StreamWriter("c:\\_my\\genom avg scores.txt");

        for (int i = 0; i < allAvgScores.Count; i++)
            file.WriteLine(allAvgScores[i].ToString());

        file.Close();
    }

    public void Update(GameTime time, Matrix view)
    {
        if (currentCar == null)
        {
            if (currentGenom < populationSize)
            {
                currentCar = new PhysicalWorld.Car(world, genoms[currentGenom]);
            }
            else
            {
                int maxIndex = 0;
                float maxScore = 0;
                for (int i = 0; i < populationSize; i++)
                {
                    if (genomScore[i] > maxScore)
                    {
                        maxScore = genomScore[i];
                        maxIndex = i;
                    }
                }

                if (maxScore > absoluteRecord)
                {
                    absoluteRecord = maxScore;
                    bestGenom = genoms[maxIndex].Clone();
                }

                allGenomsScore.Add(maxScore);

                /**********************/
                float avg = 0;
                for (int i = 0; i < populationSize; i++)
                    avg += genomScore[i];
                avg /= populationSize;
                allAvgScores.Add(avg);
                /**********************/

                for (int i = 0; i < populationSize; i++)
                    genomScore[i] = 0;

                genoms[0] = bestGenom.Clone();
                for (int i = 1; i < populationSize; i++)
                {
                    Genom.Genom mutatedGenom = bestGenom.Clone();
                    mutatedGenom.mutate();
                    genoms[i] = mutatedGenom;
                }

                currentGenom = 0;
                ++generation;
            }
        }
        else
        {
            if (!currentCar.isMove(time))
            {
                genomScore[currentGenom++] = currentCar.getScore();
                currentCar.die(world);
                currentCar = null;
            }
        }
    }

    public void drawInfo(SpriteBatch batch, SpriteFont font)
    {
        Vector2 off = new Vector2(0, 0);
        for (int i = 0; i < populationSize; i++)
            batch.DrawString(font, "Genom: " + i.ToString() + " Score: " + genomScore[i].ToString(), off + new Vector2(0, i * 16), Color.Black);
        for (int i = 0; i < allGenomsScore.Count; i++)
            batch.DrawString(font, "Generation: " + i.ToString() + " Score: " + allGenomsScore[i].ToString(), off + new Vector2(200, i * 16), Color.Black);
        for (int i = 0; i < allAvgScores.Count; i++)
            batch.DrawString(font, "Generation: " + i.ToString() + " Score: " + allAvgScores[i].ToString(), off + new Vector2(400, i * 16), Color.Black);
    }
}

namespace FarseerPhysics.HelloWorld
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        Controller ctrl;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private KeyboardState _oldKeyState;
        private SpriteFont _font;

        private Utils util;

        private World _world;

        // Simple camera controls
        private Matrix _view;
        private Vector2 _cameraPosition;
        private Vector2 _screenCenter;

        // physics simulator debug view
        DebugViewXNA _debugView;

        const string Text = "Press A or D to rotate the ball\n" +
                            "Press Space to jump\n" +
                            "Press Shift + W/S/A/D to move the camera";
        // Farseer expects objects to be scaled to MKS (meters, kilos, seconds)
        // 1 meters equals 64 pixels here
        // (Objects should be scaled to be between 0.1 and 10 meters in size)
        private const float MeterInPixels = 64f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;

            Content.RootDirectory = "Content";
            //allScores = new List<float>();
            _world = new World(new Vector2(0, 20));
            //maxScore = 0;
            util = new Utils();

            ctrl = new Controller(_world);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Initialize camera controls
            _view = Matrix.Identity;
            _cameraPosition = Vector2.Zero;

            _screenCenter = new Vector2(_graphics.GraphicsDevice.Viewport.Width / 2f,
                                                _graphics.GraphicsDevice.Viewport.Height / 2f);

            _batch = new SpriteBatch(_graphics.GraphicsDevice);
            _font = Content.Load<SpriteFont>("font");

            PhysicalWorld.Ground g = new PhysicalWorld.Ground(_world);

            // create and configure the debug view
            _debugView = new DebugViewXNA(_world);
            _debugView.AppendFlags(DebugViewFlags.DebugPanel);
            _debugView.DefaultShapeColor = Color.White;
            _debugView.SleepingShapeColor = Color.LightGray;
            _debugView.LoadContent(GraphicsDevice, Content);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleKeyboard();
            _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);
            ctrl.Update(gameTime, _view);
            base.Update(gameTime);
        }

        private void HandleKeyboard()
        {
            KeyboardState state = Keyboard.GetState();
            // Move camera
            /*if (state.IsKeyDown(Keys.A))
                _cameraPosition.X += 5.5f;

            if (state.IsKeyDown(Keys.D))
                _cameraPosition.X -= 5.5f;

            if (state.IsKeyDown(Keys.W))
                _cameraPosition.Y += 5.5f;

            if (state.IsKeyDown(Keys.S))
                _cameraPosition.Y -= 5.5f;*/

            if (ctrl.currentCar != null)
            {
                float car_x = ctrl.currentCar.getPosition().X * MeterInPixels;
                float car_y = ctrl.currentCar.getPosition().Y * MeterInPixels;
                _cameraPosition = new Vector2(-car_x, -car_y) +_screenCenter;
            }

            _view = Matrix.CreateTranslation(new Vector3(_cameraPosition - _screenCenter, 0f)) *
                    Matrix.CreateTranslation(new Vector3(_screenCenter, 0f));

            // We make it possible to rotate the circle body
            if (state.IsKeyDown(Keys.S))
            {
                /*System.IO.StreamWriter file = new System.IO.StreamWriter("f:\\test.txt");

                for (var i = 0; i < allScores.Count; i++)
                {
                    file.WriteLine(allScores[i].ToString());
                }

                file.Close();*/
            }

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            _oldKeyState = state;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, _graphics.GraphicsDevice.Viewport.Width / MeterInPixels, _graphics.GraphicsDevice.Viewport.Height / MeterInPixels, 0f, 0f,1f);
            Matrix view = Matrix.CreateTranslation(new Vector3( (_cameraPosition / MeterInPixels) - (_screenCenter / MeterInPixels), 0f) ) * Matrix.CreateTranslation(new Vector3((_screenCenter / MeterInPixels), 0f));

            _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            ctrl.drawInfo(_batch, _font);
            _batch.End();

            _debugView.RenderDebugData(ref projection, ref view);

            //_debugView.BeginCustomDraw(ref projection, ref view);
            //_debugView.EndCustomDraw();

            base.Draw(gameTime);
        }
    }
}