using System;
using Chess.ScreensManager;
using Microsoft.Xna.Framework;

namespace Chess.Core
{
    internal class Camera
    {
        private const float worldMaxDistance = 10.0f;
        private const float worldMinDistance = 5.0f;
        private const float worldMovingSpeed = 0.01f;
        private const float worldMinAngle = 30.0f;
        private const float worldMaxAngle = 85.0f;

        private readonly float aspectRatio;

        private Matrix projectionMatrix;
        private Matrix viewMatrix;
        private Matrix worldMatrix = Matrix.Identity;

        private Vector3 cameraPosition = new Vector3(0.0f, 6.0f, 4.0f);
        private Vector3 cameraTarget = Vector3.Zero;
        private Vector2 worldRotation;

        public Matrix Projection
        {
            get { return projectionMatrix; }
        }

        public Matrix View
        {
            get { return viewMatrix; }
        }

        public Matrix World
        {
            get { return worldMatrix; }
        }

        public Camera(float aspectRatio)
        {
            this.aspectRatio = aspectRatio;

            worldRotation = new Vector2(
                MathHelper.ToDegrees((float) Math.Atan(cameraPosition.Y/cameraPosition.Z)),
                0.0f);
        }

        public void Update(InputState input)
        {
            Vector2 cursorChangedPosition = new Vector2(
                input.CurrentMouseState.X - input.LastMouseState.X,
                input.CurrentMouseState.Y - input.LastMouseState.Y);

            #region Zooming

            float zoom = (float) input.ScrollWheelChange()/480;
            if (zoom < 0.0f)
            {
                if (Vector3.Distance(cameraPosition, cameraTarget) <= worldMaxDistance)
                    cameraPosition -= new Vector3(0.0f, zoom,
                                                  zoom/(float) Math.Tan(MathHelper.ToRadians(worldRotation.X)));
            }
            else if (zoom > 0.0f)
            {
                if (Vector3.Distance(cameraPosition, cameraTarget) >= worldMinDistance)
                    cameraPosition -= new Vector3(0.0f, zoom,
                                                  zoom/(float) Math.Tan(MathHelper.ToRadians(worldRotation.X)));
            }

            #endregion

            #region Moving

            if (input.IsMiddleButtonPressing())
            {
                Vector3 changeLook = new Vector3(cursorChangedPosition.X, 0.0f,
                                                 cursorChangedPosition.Y);
                cameraPosition -= changeLook*worldMovingSpeed;
                cameraTarget -= changeLook*worldMovingSpeed;
            }

            #endregion

            #region Rotating

            if (input.IsRightButtonPressing())
            {
                worldRotation.Y += cursorChangedPosition.X;
                worldMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(worldRotation.Y));

                worldRotation.X += cursorChangedPosition.Y;
                worldRotation.X = MathHelper.Clamp(worldRotation.X, worldMinAngle, worldMaxAngle);
                float distanceToTarget = Vector3.Distance(cameraPosition, cameraTarget);
                float distanceX = distanceToTarget*(float) Math.Cos(MathHelper.ToRadians(worldRotation.X));
                float distanceY = distanceToTarget*(float) Math.Sin(MathHelper.ToRadians(worldRotation.X));
                Ray rayX = new Ray(cameraTarget, Vector3.Normalize(
                    new Vector3(cameraPosition.X, 0.0f, cameraPosition.Z) - cameraTarget));
                Ray rayY = new Ray(cameraTarget, Vector3.Up);
                cameraPosition.X = (rayX.Position + rayX.Direction*distanceX).X;
                cameraPosition.Z = (rayX.Position + rayX.Direction*distanceX).Z;
                cameraPosition.Y = (rayY.Position + rayY.Direction*distanceY).Y;
            }

            #endregion

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), aspectRatio, .01f, 1000.0f);
            viewMatrix = Matrix.CreateLookAt(cameraPosition,
                                             cameraTarget, Vector3.Up);
        }
    }
}