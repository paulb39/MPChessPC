using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Chess.Board;
using Chess.Core;
using Chess.ScreensManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TrianglePipeline;

namespace Chess.Screens
{
    //TODO refactor (less code)
    /// <summary>
    /// Screen where game occurs
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        #region Fields & Constants

        private const float cellSize = 0.63f;

        private ContentManager content;
        private SpriteBatch spriteBatch;

        #region Models

        // the chessboard that all of the objects are drawn on, and chessboard model's 
        // absoluteBoneTransforms. Since the chessboard is not animated, these can be 
        // calculated once and saved.
        private Model chessboardModel;
        private Matrix[] chessboardAbsoluteBoneTransforms;

        // these are the models that we will draw on top of the chessboard. we'll store them
        // and their bone transforms in arrays. Again, since these models aren't
        // animated, we can calculate their bone transforms once and save the result.
        private readonly Model[] figureModels = new Model[BoardState.FiguresNumber];
        private readonly Matrix[][] figureModelAbsoluteBoneTransforms = new Matrix[BoardState.FiguresNumber][];
        // each model will need one more matrix: a world transform. This matrix will be
        // used to place each model at a different location in the world.
        private readonly Matrix[] figureModelWorldTransforms = new Matrix[BoardState.FiguresNumber];

        #endregion

        #region Chessboard

        private BoardState boardState;

        #endregion

        #region Type moving

        // Index of figure that movies by mouse at this moment
        private int? movingFigureIndex;
        // Variable in use when figure is moving over chessboard
        private Vector3 figureSphereCenter;

        /// <summary>
        /// Save position of mouse (after corection) when figure was just selected
        /// </summary>
        private Point mousePoisitionOnPickup = default(Point);

        #endregion

        #region Cursor

        private Vector2 cursorPosition;

        #endregion

        #region Cell highlight

        private BasicEffect cellBasicEffect;
        private Texture2D cellTexture;
        private VertexPositionTexture[] cellVertices;

        #endregion

        #region Camera class

        private Camera camera;

        #endregion

        #endregion

        private readonly ProcessStartInfo engineProcessInfo = new ProcessStartInfo
                                                                  {
                                                                      CreateNoWindow = true,
                                                                      WindowStyle = ProcessWindowStyle.Hidden,
                                                                      ErrorDialog = false,
                                                                      UseShellExecute = false,
                                                                      RedirectStandardOutput = true,
                                                                      RedirectStandardInput = true
                                                                  };

        #region Initialization

        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            boardState = new BoardState(ScreenManager.ComputerColorArray.Current, this);

            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            camera = new Camera(ScreenManager.GraphicsDevice.Viewport.Width/
                                (float) ScreenManager.GraphicsDevice.Viewport.Height);

            #region Load chessboard and figures

            // Load all of the models that will appear on the chessboard:
            for (var i = 0; i < BoardState.FiguresNumber; i++)
                if (boardState.CheckFigureExistence(i))
                {
                    // Load the actual model, using BoardState to determine what
                    // file to load.
                    figureModels[i] =
                        content.Load<Model>(string.Format(@"Models\{0}", boardState.GetFigure(i).GetModelName()));

                    // Create an array of matrices to hold the absolute bone transforms,
                    // calculate them, and copy them in.
                    figureModelAbsoluteBoneTransforms[i] =
                        new Matrix[figureModels[i].Bones.Count];
                    figureModels[i].CopyAbsoluteBoneTransformsTo(
                        figureModelAbsoluteBoneTransforms[i]);
                }

            // Now that we've loaded in the models that will sit on the chessboard, go
            // through the same procedure for the chessboard itself.
            chessboardModel = content.Load<Model>(@"Models\Chessboard");
            chessboardAbsoluteBoneTransforms = new Matrix[chessboardModel.Bones.Count];
            chessboardModel.CopyAbsoluteBoneTransformsTo(chessboardAbsoluteBoneTransforms);

            #endregion

            #region Cell highlight

            cellBasicEffect = new BasicEffect(ScreenManager.GraphicsDevice);

            cellVertices = new VertexPositionTexture[9];

            cellTexture = content.Load<Texture2D>(@"Textures\Cell");

            #endregion

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }

        public void ShowFiguresBox()
        {
            var chooseBox = new ChooseFigureBoxScreen();
            chooseBox.FigureChosen += (sender, e) => boardState.ChangePawnOnBoard(e.FigureType);
            ScreenManager.AddScreen(chooseBox);
        }

        public void ReloadFigureModel(int number, String modelName)
        {
            // Load the actual model, using BoardState to determine what
            // file to load.
            figureModels[number] =
                content.Load<Model>(string.Format(@"Models\{0}", modelName));

            // Create an array of matrices to hold the absolute bone transforms,
            // calculate them, and copy them in.
            figureModelAbsoluteBoneTransforms[number] =
                new Matrix[figureModels[number].Bones.Count];
            figureModels[number].CopyAbsoluteBoneTransformsTo(
                figureModelAbsoluteBoneTransforms[number]);
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }

        #endregion

        #region Update

        private void CellHighlightPosition()
        {
            if (!movingFigureIndex.HasValue)
                return;

            var modelPosition = figureModelWorldTransforms[movingFigureIndex.Value].Translation;
            var cellPositionX = (float) Math.Floor(modelPosition.X/cellSize) + 1;
            var cellPositionZ = (float) Math.Floor(modelPosition.Z/cellSize);
            cellPositionX = MathHelper.Clamp(cellPositionX, -3.0f, 4.0f);
            cellPositionZ = MathHelper.Clamp(cellPositionZ, -4.0f, 3.0f);
            var cellWorldPosition = cellSize*new Vector3(
                                                 cellPositionX, 0.0f, cellPositionZ);

            var j = 0;
            cellVertices[j++] = new VertexPositionTexture(new Vector3(0, 0, 0) +
                                                          cellWorldPosition, new Vector2(0, 1));
            cellVertices[j++] = new VertexPositionTexture(new Vector3(0, 0, cellSize) +
                                                          cellWorldPosition, new Vector2(0, 0));
            cellVertices[j++] = new VertexPositionTexture(new Vector3(-cellSize, 0, cellSize) +
                                                          cellWorldPosition, new Vector2(1, 0));

            cellVertices[j++] = new VertexPositionTexture(new Vector3(-cellSize, 0, cellSize) +
                                                          cellWorldPosition, new Vector2(1, 0));
            cellVertices[j++] = new VertexPositionTexture(new Vector3(-cellSize, 0, 0) +
                                                          cellWorldPosition, new Vector2(1, 1));
            cellVertices[j] = new VertexPositionTexture(new Vector3(0, 0, 0) +
                                                        cellWorldPosition, new Vector2(0, 1));
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Camera updates only by mouse.
            camera.Update(input);

            // Save current mouse position for draw method
            cursorPosition = new Vector2(input.CurrentMouseState.X, input.CurrentMouseState.Y);

            // If paused - show pause menu and do not  moving figures.
            if (input.IsPauseGame())
            {
                ScreenManager.AddScreen(new PauseMenuScreen());
            }
            else
            {
                #region Pick up figure above chessboard

                SortedList<float, int> intersectionFigures;
                if (input.IsLeftButtonPressed())
                    if ((intersectionFigures = ChooseFigure()).Count > 0)
                        for (var i = 0; i < intersectionFigures.Count; i++)
                            if (ModelRayCollision(figureModels[intersectionFigures.Values[i]],
                                                  figureModelWorldTransforms[intersectionFigures.Values[i]]))
                            {
                                movingFigureIndex = intersectionFigures.Values[i];

                                figureSphereCenter = BoundingSphereCenter(
                                    figureModels[movingFigureIndex.Value],
                                    figureModelWorldTransforms[movingFigureIndex.Value],
                                    figureModelAbsoluteBoneTransforms[movingFigureIndex.Value]);

                                // Move cursor with figure
                                var newMouseScreenPosition =
                                    ScreenManager.GraphicsDevice.Viewport.Project(figureSphereCenter,
                                                                                  camera.Projection, camera.View,
                                                                                  camera.World);
                                Mouse.SetPosition((int) newMouseScreenPosition.X, (int) newMouseScreenPosition.Y);
                                mousePoisitionOnPickup = new Point((int) newMouseScreenPosition.X,
                                                                   (int) newMouseScreenPosition.Y);
                                break;
                            }

                #endregion

                #region Move figure over chessboard

                if (input.IsLeftButtonPressing())
                {
                    var mouseState = Mouse.GetState();
                    /* Do not move figure until cursor is moved after pick up */
                    if (!(mousePoisitionOnPickup.X == mouseState.X &&
                          mousePoisitionOnPickup.Y == mouseState.Y))
                        if (movingFigureIndex.HasValue)
                        {
                            var newRay = GetPickRay();
                            var movingPoint =
                                RayPlaneIntersectionPoint(newRay,
                                                          new Plane(new Vector4(0.0f, 1.0f, 0.0f, -figureSphereCenter.Y)));
                            if (movingPoint.HasValue)
                            {
                                figureModelWorldTransforms[movingFigureIndex.Value] *=
                                    Matrix.CreateTranslation(
                                        new Vector3(movingPoint.Value.X - figureSphereCenter.X, 0.0f,
                                                    movingPoint.Value.Z - figureSphereCenter.Z));

                                figureSphereCenter = BoundingSphereCenter(
                                    figureModels[movingFigureIndex.Value],
                                    figureModelWorldTransforms[movingFigureIndex.Value],
                                    figureModelAbsoluteBoneTransforms[movingFigureIndex.Value]);
                            }
                        }
                }

                #endregion

                #region Put figure on chessboard back

                if (input.IsLeftButtonReleased())
                {
                    if (movingFigureIndex.HasValue)
                    {
                        var modelPosition = figureModelWorldTransforms[movingFigureIndex.Value].Translation;
                        var figurePosition = new FigurePosition(
                            (char) ('a' + (int) Math.Floor(modelPosition.X/cellSize) + 4),
                            -(int) Math.Floor(modelPosition.Z/cellSize) + 4);
                        var figureMoved = boardState.PlayerMove(movingFigureIndex.Value, figurePosition);

                        /* No figure is selected now */
                        movingFigureIndex = null;

                        /* Computer's turn */
                        if (figureMoved && boardState.CurrentMoveColor == ScreenManager.ComputerColorArray.Current)
                        {
                            new Thread(() =>
                                           {
                                               switch (ScreenManager.GameLevelsArray.Current)
                                               {
                                                   case ScreenManager.GameLevels.Easy:
                                                       engineProcessInfo.FileName = @"Engines\ace.exe";
                                                       break;
                                                   case ScreenManager.GameLevels.Normal:
                                                       engineProcessInfo.FileName = @"Engines\tarrasch_0.906.exe";
                                                       break;
                                                   case ScreenManager.GameLevels.Hard:
                                                       engineProcessInfo.FileName = @"Engines\tankist_3.1.exe";
                                                       break;
                                                   default:
                                                       throw new Exception("Unknown game level");
                                               }
                                               var process = Process.Start(engineProcessInfo);
                                               process.StandardInput.WriteLine("uci");
                                               process.StandardInput.WriteLine(string.Format("position fen {0}",
                                                                                             boardState.GenerateFenState
                                                                                                 ()));
                                               process.StandardInput.WriteLine(string.Format("go movetime 1000"));

                                               process.OutputDataReceived += (sender, outputLine) =>
                                                                                 {
                                                                                     if (outputLine.Data == null)
                                                                                         return;

                                                                                     if (
                                                                                         !outputLine.Data.Contains(
                                                                                             "bestmove"))
                                                                                         return;

                                                                                     var move =
                                                                                         outputLine.Data.Split(' ')[1];
                                                                                     process.StandardInput.WriteLine(
                                                                                         "quit");
                                                                                     process.Dispose();
                                                                                     boardState.ComputerMove(move);
                                                                                 };
                                               process.BeginOutputReadLine();
                                           }
                                ) {IsBackground = true}.Start();
                        }
                    }
                }

                #endregion
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.BurlyWood);

            spriteBatch = ScreenManager.SpriteBatch;

            #region Draw chessboard and figures

            // For correct drawing 3D models
            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Draw the table. DrawModel is a function defined below draws a model using
            // a world matrix and the model's bone transforms.
            DrawModel(chessboardModel, camera.World, chessboardAbsoluteBoneTransforms);

            // Calculate current position of figures, except moving figure, and
            // use the same DrawModel function to draw all of the models on the table.
            for (var i = 0; i < BoardState.FiguresNumber; i++)
                if (boardState.CheckFigureExistence(i))
                {
                    if (i != movingFigureIndex)
                    {
                        // Set position of figure in game world
                        figureModelWorldTransforms[i] =
                            Matrix.CreateTranslation(new Vector3(
                                                         (boardState.FigurePosition(i).X - 'a' - 3.5f)*cellSize,
                                                         0.0f,
                                                         -(boardState.FigurePosition(i).Y - 4.5f)*cellSize));
                    }
                    DrawModel(figureModels[i], figureModelWorldTransforms[i]*camera.World,
                              figureModelAbsoluteBoneTransforms[i]);
                }

            #endregion

            #region Cell highlight

            DrawGameInfo(gameTime); //TODO Message logic - add if in check, players name etc

            // Where we want to draw?
            CellHighlightPosition();
            // Let's draw!
            if (movingFigureIndex.HasValue)
            {
                ScreenManager.GraphicsDevice.BlendState = BlendState.AlphaBlend;

                ScreenManager.GraphicsDevice.RasterizerState = new RasterizerState()
                                                                   {
                                                                       CullMode = CullMode.None
                                                                   };
                cellBasicEffect.World = camera.World*
                                        Matrix.CreateTranslation(Vector3.Up/200);
                cellBasicEffect.View = camera.View;
                cellBasicEffect.Projection = camera.Projection;
                cellBasicEffect.Texture = cellTexture;
                cellBasicEffect.TextureEnabled = true;

                foreach (var pass in cellBasicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    ScreenManager.GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(
                        PrimitiveType.TriangleList,
                        cellVertices,
                        0,
                        2);
                }

                ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            }

            #endregion

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }

        private void DrawGameInfo(GameTime gameTime) //TODO add if in check etc
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            var currentPlayerMessage = (GameState.CurrentPlayerMove == FigureColors.White) ? "Your move!" : "Opponent's turn";
            ///Console.WriteLine(currentPlayerMessage);

            //TODO add null logic
//            MenuEntry gameInfoStatus = new MenuEntry(currentPlayerMessage); // different class?
//            gameInfoStatus.Selected += (sender, e) => { Console.WriteLine("You clicked the info text"); };

            var viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 position =  new Vector2((viewport.Width / 2f) - 75, viewport.Height / 100f); //325, 4.8 //Centered on top of board

            spriteBatch.Begin();
            spriteBatch.DrawString(font, currentPlayerMessage, position, Color.Red);
            spriteBatch.End();

        }

        /// <summary>
        /// DrawModel is a helper function that takes a model, world matrix, and
        /// bone transforms. It does just what its name implies, and draws the model.
        /// </summary>
        /// <param name="model">the model to draw</param>
        /// <param name="worldTransform">where to draw the model</param>
        /// <param name="absoluteBoneTransforms">the model's bone transforms. this can
        /// be calculated using the function Model.CopyAbsoluteBoneTransformsTo</param>
        private void DrawModel(Model model, Matrix worldTransform,
                               Matrix[] absoluteBoneTransforms)
        {
            // nothing tricky in here; this is the same model drawing code that we see
            // everywhere. we'll loop over all of the meshes in the model, set up their
            // effects, and then draw them.
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    effect.World = absoluteBoneTransforms[mesh.ParentBone.Index]*
                                   worldTransform;
                }
                mesh.Draw();
            }
        }

        #endregion

        #region Helper functions

        #region Type's center (for moving cursor)

        /// <summary>
        /// Find bounding sphere center. if it is several spheres,
        /// find arichmetic middle.
        /// </summary>
        /// <param name="model">the model to perform the intersection check with.
        /// the model's bounding spheres will be used.</param>
        /// <param name="worldTransform">a matrix that positions the model
        /// in world space</param>
        /// <param name="absoluteBoneTransforms">this array of matrices contains the
        /// absolute bone transforms for the model. this can be obtained using the
        /// Model.CopyAbsoluteBoneTransformsTo function.</param>
        /// <returns>Center of model's bounding sphere</returns>
        private static Vector3 BoundingSphereCenter(Model model,
                                                    Matrix worldTransform, Matrix[] absoluteBoneTransforms)
        {
            Vector3 sumOfCenters = Vector3.Zero;

            foreach (ModelMesh mesh in model.Meshes)
            {
                // the mesh's BoundingSphere is stored relative to the mesh itself.
                // (Mesh space). We want to get this BoundingSphere in terms of world
                // coordinates. To do this, we calculate a matrix that will transform
                // from coordinates from mesh space into world space....
                Matrix world =
                    absoluteBoneTransforms[mesh.ParentBone.Index]*worldTransform;

                // ... and then transform the BoundingSphere using that matrix.
                BoundingSphere sphere =
                    TransformBoundingSphere(mesh.BoundingSphere, world);

                sumOfCenters += sphere.Center;
            }

            return sumOfCenters/model.Meshes.Count;
        }

        #endregion

        #region Ray functions

        /// <summary>
        /// Find a ray which appropriate 2D mouse position on screen.
        /// </summary>
        /// <returns>Ray of points which appropriate mouse cursor.</returns>
        private Ray GetPickRay()
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearsource =
                new Vector3(cursorPosition, 0f);
            Vector3 farsource =
                new Vector3(cursorPosition, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = ScreenManager.GraphicsDevice.Viewport.Unproject(nearsource,
                                                                                camera.Projection, camera.View,
                                                                                camera.World);

            Vector3 farPoint = ScreenManager.GraphicsDevice.Viewport.Unproject(farsource,
                                                                               camera.Projection, camera.View,
                                                                               camera.World);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it...
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        /// <summary>
        /// Find point at which the ray intersects the plane.
        /// </summary>
        /// <param name="ray">Ray</param>
        /// <param name="height">Point height from chessboard</param>
        /// <returns>Return intersection point or null</returns>
        private static Vector3? RayPlaneIntersectionPoint(Ray ray, Plane plane)
        {
            float? intersection;
            intersection = ray.Intersects(plane);
            if (intersection.HasValue)
                return ray.Position + intersection.Value*ray.Direction;
            else
                return null;
        }

        #endregion

        #region Model-Ray Collision (BoundingSphere)

        /// <summary>
        /// Choosing figures by it's spheres.
        /// </summary>
        /// <returns>SortedList, where keys are distance and values are models' indexes.</returns>
        private SortedList<float, int> ChooseFigure()
        {
            // If the cursor is over a model, we'll take it up. To figure out if
            // the cursor is over a model, we'll use GetPickRay. That
            // function gives us a world space ray that starts at the "eye" of the
            // camera, and shoots out in the direction pointed to by the cursor.
            Ray cursorRay = GetPickRay();

            SortedList<float, int> modelsIndexesAndDistance = new SortedList<float, int>();

            // go through all of the models...
            for (int i = 0; i < figureModels.Length; i++)
                if (boardState.CheckFigureExistence(i))
                {
                    // check to see if the cursorRay intersects the model....
                    float? result =
                        RayIntersectsModelSphere(cursorRay, figureModels[i], figureModelWorldTransforms[i],
                                                 figureModelAbsoluteBoneTransforms[i]);
                    if (result.HasValue)
                        modelsIndexesAndDistance.Add(result.Value, i);
                }

            return modelsIndexesAndDistance;
        }

        /// <summary>
        /// This helper function checks to see if a ray will intersect with a model.
        /// The model's bounding spheres are used, and the model is transformed using
        /// the matrix specified in the worldTransform argument.
        /// </summary>
        /// <param name="ray">the ray to perform the intersection check with</param>
        /// <param name="model">the model to perform the intersection check with.
        /// the model's bounding spheres will be used.</param>
        /// <param name="worldTransform">a matrix that positions the model
        /// in world space</param>
        /// <param name="absoluteBoneTransforms">this array of matrices contains the
        /// absolute bone transforms for the model. this can be obtained using the
        /// Model.CopyAbsoluteBoneTransformsTo function.</param>
        /// <returns>true if the ray intersects the model.</returns>
        private static float? RayIntersectsModelSphere(Ray ray, Model model,
                                                       Matrix worldTransform, Matrix[] absoluteBoneTransforms)
        {
            float? modelDistance = null;
            // Each ModelMesh in a Model has a bounding sphere, so to check for an
            // intersection in the Model, we have to check every mesh.
            foreach (ModelMesh mesh in model.Meshes)
            {
                // the mesh's BoundingSphere is stored relative to the mesh itself.
                // (Mesh space). We want to get this BoundingSphere in terms of world
                // coordinates. To do this, we calculate a matrix that will transform
                // from coordinates from mesh space into world space....
                Matrix world =
                    absoluteBoneTransforms[mesh.ParentBone.Index]*worldTransform;

                // ... and then transform the BoundingSphere using that matrix.
                BoundingSphere sphere =
                    TransformBoundingSphere(mesh.BoundingSphere, world);

                // now that the we have a sphere in world coordinates, we can just use
                // the BoundingSphere class's Intersects function. Intersects returns a
                // nullable float (float?). This value is the distance at which the ray
                // intersects the BoundingSphere, or null if there is no intersection.
                float? distance = sphere.Intersects(ray);
                if (distance.HasValue)
                {
                    if (!modelDistance.HasValue)
                        modelDistance = distance;
                    else
                        modelDistance =
                            MathHelper.Min(modelDistance.Value, distance.Value);
                }
            }

            return modelDistance;
        }

        /// <summary>
        /// This helper function takes a BoundingSphere and a transform matrix, and
        /// returns a transformed version of that BoundingSphere.
        /// </summary>
        /// <param name="sphere">the BoundingSphere to transform</param>
        /// <param name="world">how to transform the BoundingSphere.</param>
        /// <returns>the transformed BoundingSphere/</returns>
        private static BoundingSphere TransformBoundingSphere(BoundingSphere sphere,
                                                              Matrix transform)
        {
            BoundingSphere transformedSphere;

            // the transform can contain different scales on the x, y, and z components.
            // this has the effect of stretching and squishing our bounding sphere along
            // different axes. Obviously, this is no good: a bounding sphere has to be a
            // SPHERE. so, the transformed sphere's radius must be the maximum of the 
            // scaled x, y, and z radii.

            // to calculate how the transform matrix will affect the x, y, and z
            // components of the sphere, we'll create a vector3 with x y and z equal
            // to the sphere's radius...
            Vector3 scale3 = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);

            // then transform that vector using the transform matrix. we use
            // TransformNormal because we don't want to take translation into account.
            scale3 = Vector3.TransformNormal(scale3, transform);

            // scale3 contains the x, y, and z radii of a squished and stretched sphere.
            // we'll set the finished sphere's radius to the maximum of the x y and z
            // radii, creating a sphere that is large enough to contain the original 
            // squished sphere.
            transformedSphere.Radius = Math.Max(scale3.X, Math.Max(scale3.Y, scale3.Z));

            // transforming the center of the sphere is much easier. we can just use 
            // Vector3.Transform to transform the center vector. notice that we're using
            // Transform instead of TransformNormal because in this case we DO want to 
            // take translation into account.
            transformedSphere.Center = Vector3.Transform(sphere.Center, transform);

            return transformedSphere;
        }

        #endregion

        #region Model-Ray collision (Triangles)

        private bool ModelRayCollision(Model model, Matrix modelWorld)
        {
            Ray ray = GetPickRay();

            Matrix[] modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            bool collision = false;
            foreach (ModelMesh mesh in model.Meshes)
            {
                Matrix absTransform = modelTransforms[mesh.ParentBone.Index]*modelWorld;
                Triangle[] meshTriangles = (Triangle[]) mesh.Tag;

                Matrix invMatrix = Matrix.Invert(absTransform);
                Vector3 transRayStartPoint = Vector3.Transform(ray.Position, invMatrix);
                Vector3 origRayEndPoint = ray.Position + ray.Direction;
                Vector3 transRayEndPoint = Vector3.Transform(origRayEndPoint, invMatrix);
                Ray invRay = new Ray(transRayStartPoint, transRayEndPoint - transRayStartPoint);

                foreach (Triangle tri in meshTriangles)
                {
                    Plane trianglePlane = new Plane(tri.P0, tri.P1, tri.P2);

                    float distanceOnRay = RayPlaneIntersection(invRay, trianglePlane);
                    Vector3 intersectionPoint = invRay.Position + distanceOnRay*invRay.Direction;

                    if (PointInsideTriangle(tri.P0, tri.P1, tri.P2, intersectionPoint))
                        collision = true;
                }
            }

            return collision;
        }

        private static float RayPlaneIntersection(Ray ray, Plane plane)
        {
            float rayPointDist = -plane.DotNormal(ray.Position);
            float rayPointToPlaneDist = rayPointDist - plane.D;
            float directionProjectedLength = Vector3.Dot(plane.Normal, ray.Direction);
            float factor = rayPointToPlaneDist/directionProjectedLength;
            return factor;
        }

        private static bool PointInsideTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 point)
        {
            if (float.IsNaN(point.X)) return false;

            Vector3 A0 = point - p0;
            Vector3 B0 = p1 - p0;
            Vector3 cross0 = Vector3.Cross(A0, B0);

            Vector3 A1 = point - p1;
            Vector3 B1 = p2 - p1;
            Vector3 cross1 = Vector3.Cross(A1, B1);

            Vector3 A2 = point - p2;
            Vector3 B2 = p0 - p2;
            Vector3 cross2 = Vector3.Cross(A2, B2);

            if (CompareSigns(cross0, cross1) && CompareSigns(cross0, cross2))
                return true;
            else
                return false;
        }

        private static bool CompareSigns(Vector3 first, Vector3 second)
        {
            if (Vector3.Dot(first, second) > 0)
                return true;
            else
                return false;
        }

        #endregion

        #endregion
    }
}