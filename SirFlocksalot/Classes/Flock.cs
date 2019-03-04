using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SirFlocksalot
{
    public class Flock
    {
        static public int NumFlock = 57;
        static public int NumRogue = 3;
        List<Agent> Agents = new List<Agent>();
        public Flock() { }
        public void CreateFlock(List<Texture2D> PetalTextures, Texture2D RogueTexture)
        {
            for (int i = 0; i < NumFlock; i++)
            {
                Agents.Add(new FlockAgent(FlockTools.Pick(PetalTextures)));
            }
            for (int i = 0; i < NumRogue; i++)
            {
                Agents.Add(new RogueAgent(RogueTexture));
            }
        }

        public void Update(float CurrentTime, float DeltaTime, float TimeModifier)
        {
            foreach (Agent a in Agents)
            {
                a.Update(CurrentTime, DeltaTime, TimeModifier, Agents);
            }
        }
        public void Draw(SpriteBatch SB)
        {
            foreach (Agent a in Agents)
            {
                a.Draw(SB);
            }
        }
    }
}
