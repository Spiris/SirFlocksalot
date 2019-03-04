using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SirFlocksalot
{
    public class Moon : GameObject
    {
        readonly float Speed = 0.00005f;
        readonly Vector2 AnchorPosition = new Vector2(1200, 5200);
        float Orientation = 0.0f;        
        public Texture2D Texture;

        public Moon()
        {
            Orientation = (float)Math.PI * 1.93f;
            Position = new Vector2(AnchorPosition.X + ((float)Math.Cos(Orientation) + 5100*(float)Math.Sin(Orientation)), AnchorPosition.Y + (-5100*(float)Math.Cos(Orientation) + (float)Math.Sin(Orientation)));
        }
        public void Update(float DeltaTime)
        {
            Position = new Vector2(AnchorPosition.X + ((float)Math.Cos(Orientation) + 5100 * (float)Math.Sin(Orientation)), AnchorPosition.Y + (-5100 * (float)Math.Cos(Orientation) + (float)Math.Sin(Orientation)));
            Orientation = MathHelper.WrapAngle(Orientation + DeltaTime * Speed);
        }

        internal void Draw(SpriteBatch SB)
        {
            SB.Draw(Texture, Position, Color.White);
        }
    }
}
