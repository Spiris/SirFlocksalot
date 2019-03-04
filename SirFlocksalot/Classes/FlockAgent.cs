using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace SirFlocksalot
{    
    public class Agent : GameObject
    {
        static int CurrentAgentId = 0;

        protected Texture2D Texture;
        public int AgentId = 0;
        public bool IsRogue = false;
        public Vector2 Velocity = Vector2.One;
        protected Vector2 Acceleration = Vector2.Zero;
        protected float Orientation = 0.0f;
        protected float MaxForce = 5f;
        protected float MaxForceSqared = 0.0f;
        protected float MaxSpeed = 100.0f;
        protected float MaxSpeedSquared = 0.0f;
        protected float MaxTurnRate = MathHelper.TwoPi / 10.0f;


        public Agent()
        {
            AgentId = CurrentAgentId++;
            MaxForceSqared = MaxForce * MaxForce;
            MaxSpeedSquared = MaxSpeed * MaxSpeed;
            Position = new Vector2(FlockTools.GetRandomFloat(SirFlocksalotGame.ScreenSize.X), FlockTools.GetRandomFloat(SirFlocksalotGame.ScreenSize.Y));
            float QuarterSpeed = MaxSpeed * 0.25f;
            Velocity = FlockTools.GetRandomVector2(-QuarterSpeed, QuarterSpeed, -QuarterSpeed, QuarterSpeed);
            Orientation = Velocity.Heading();
        }
        public virtual void Update(float CurrentTime, float DeltaTime, float TimeModifier, List<Agent> Agents)
        {
            FlockTools.Limit(ref Acceleration, MaxForceSqared, MaxForce);
            Velocity += Acceleration;
            FlockTools.Limit(ref Velocity, MaxSpeedSquared, MaxSpeed);
            if(Velocity.LengthSquared() > 1)
            {
                Orientation = Velocity.Heading();
            }
            Position += Velocity * DeltaTime;
            FlockTools.WrapVector(ref Position, SirFlocksalotGame.ScreenSize.ToVector2(), 100);
            Acceleration *= 0.9f;
        }
        public virtual void Draw(SpriteBatch SB) { }
        protected Vector2 Seek(Vector2 Target)
        {
            Vector2 desiredVelocity = Vector2.Subtract(Target, Position);
            desiredVelocity.Normalize();
            float desiredHeading = desiredVelocity.Heading();
            float headingDiff = desiredHeading - Orientation;
            if(headingDiff > Math.PI)
            {
                headingDiff = -(MathHelper.TwoPi - headingDiff);
            }
            else if (headingDiff < -Math.PI)
            {
                headingDiff = MathHelper.TwoPi - Math.Abs(headingDiff);
            }
            float turnDelta = MathHelper.Clamp(headingDiff, -MaxTurnRate, MaxTurnRate);
            float desire = Orientation + turnDelta;            
            return new Vector2((float)Math.Cos(desire) * MaxSpeed, (float)Math.Sin(desire) * MaxSpeed);
        }
    }
    public class FlockAgent : Agent
    {        
        int NumNeighbors = 0;
        float FlockDistance = 0;
        float FlockDistanceSqared = 0;
        float FlockAngle = 0;
        float CohesionWeight = 0;
        float SeparationWeight = 0;
        float AlignmentWeight = 0;
        float PerlinBeat = 0;
        float PRadius = 50;
        float PTheta = 0;
        float POrientation = 0;
        float ColorFalloff = 0;
        Color PetalColor = Color.White;
        Vector2 DrawPosition = Vector2.Zero;
        public FlockAgent(Texture2D AgentTexture) : base()
        {
            Texture = AgentTexture;
            MaxForce = 10;
            MaxForceSqared = MaxForce * MaxForce;
            FlockDistance = 80 + FlockTools.GetRandomFloat(30.0f);
            FlockDistanceSqared = FlockDistance * FlockDistance;
            FlockAngle = (float)Math.PI - FlockTools.GetRandomFloat((float)Math.PI * 0.5f);
            CohesionWeight = 0.3f + FlockTools.GetRandomFloat(0.3f) - 0.1f;
            SeparationWeight = 0.2f + FlockTools.GetRandomFloat(0.25f) - 0.1f;
            AlignmentWeight = 0.3f + FlockTools.GetRandomFloat(0.25f) - 0.05f;
            PTheta = FlockTools.GetRandomFloat(MathHelper.TwoPi);
            PerlinBeat = FlockTools.GetRandomFloat(-0.01f, 0.01f);
        }
        public override void Update(float CurrentTime, float DeltaTime, float TimeModifier, List<Agent> Agents)
        {
            float mod_DT = DeltaTime * TimeModifier;
            UpdateColor(mod_DT);
            List<Agent> neighbors = FindNeighbors(Agents);
            Flit(CurrentTime, mod_DT);
            Vector2 flockingForce = Flock(neighbors);                
            Acceleration += flockingForce;           
            base.Update(CurrentTime, mod_DT, TimeModifier, Agents);
            float cosTheta = (float)Math.Cos(PTheta) * PRadius;
            float sinTheta = (float)Math.Sin(PTheta) * PRadius;
            DrawPosition = Position + new Vector2(cosTheta - sinTheta, cosTheta + sinTheta);
        }
        void UpdateColor(float DeltaTime)
        {
            float AdditiveChange = (NumNeighbors - 1) * 20 * DeltaTime;
            ColorFalloff = MathHelper.Clamp(ColorFalloff + AdditiveChange, 0, 200);
            int RGBValue = (int)ColorFalloff + 55;
            PetalColor = new Color(RGBValue, RGBValue, RGBValue, 255);
        }
        private List<Agent> FindNeighbors(List<Agent> Agents)
        {
            List<Agent> nearby = new List<Agent>();
            float a1 = -FlockAngle;
            float a2 = FlockAngle;
            Vector2 dir = FlockTools.GetSafeNormal(Velocity);
            foreach(Agent a in Agents)
            {
                if(AgentId != a.AgentId)
                {
                    Vector2 toNeighbor = Vector2.Subtract(a.Position, Position);
                    float dsq = toNeighbor.LengthSquared();
                    if(dsq < FlockDistanceSqared)
                    {
                        toNeighbor.Normalize();
                        float dotProduct = Vector2.Dot(dir, toNeighbor);
                        float theta = (float)Math.Acos(dotProduct);
                        if(theta < FlockAngle)
                        {
                            nearby.Add(a);
                        }
                    }
                }
            }
            NumNeighbors = nearby.Count;
            return nearby;
        }

        void Flit(float CurrentTime, float DeltaTime)
        {
            //POrientation = MathHelper.WrapAngle(POrientation);
            PerlinBeat = MathHelper.Clamp(PerlinBeat + FlockTools.GetRandomFloat(-0.05f, 0.05f), -1f, 1f);
            float perlinR = (Noise.GetNoise(CurrentTime * 100, 0, 0)) * DeltaTime * PerlinBeat;
            PTheta = MathHelper.WrapAngle(PTheta + perlinR);
            POrientation += perlinR;
        }
        Vector2 Flock(List<Agent> Neighbors)
        {            
            if (Neighbors.Count == 0)
                return Vector2.Zero;
            Vector2 steer = Vector2.Zero;
            Vector2 alignment = Vector2.Zero;
            Vector2 separation = Vector2.Zero;
            Vector2 cohesion = Vector2.Zero;
            Vector2 cv = Vector2.Zero;
            foreach(Agent a in Neighbors)
            {
                float distSq = Vector2.DistanceSquared(Position, a.Position);
                float t = FlockTools.Map(distSq, 0, FlockDistanceSqared, 1, 0);
                Vector2 dir = Vector2.Multiply(a.Velocity, t);
                if(a.IsRogue)
                {
                    steer += Seek(a.Position + a.Velocity * 10) * CohesionWeight;
                    return steer;
                }
                alignment += dir;
                Vector2 sep = Vector2.Subtract(Position, a.Position);
                float r = sep.LengthSquared();
                sep.Normalize();
                sep *= 1.0f / r;
                separation += sep;
                cv += a.Position;
            }
            alignment /= Neighbors.Count;
            alignment.Normalize();
            steer += alignment * AlignmentWeight;

            cv /= Neighbors.Count;
            cohesion = Seek(cv);
            cohesion.Normalize();
            steer += cohesion * CohesionWeight;

            separation.Normalize();
            steer += separation * SeparationWeight;
            return steer;
        }
        public override void Draw(SpriteBatch SB)
        {
            SB.Draw(Texture, DrawPosition, Texture.Bounds, PetalColor, POrientation * MathHelper.TwoPi, Vector2.One, new Vector2(0.5f, 0.5f), SpriteEffects.None, 0);
        }
    }
    public class RogueAgent : Agent
    {
        class PastPosition
        {
            public Color Color = Color.White;
            public Vector2 Position = Vector2.Zero;
        }
        float WanderStrength = 10;
        float WanderAmp = 15000;
        float WanderDistance = 100;
        float WanderRadius = 0;
        float WanderRate = 0.01f;
        float WanderTheta = 0;
        float WanderDelta = 0;
        float SeekStrength = 2;
        float DilationDistance = 150;
        float DilationDistanceSquared = 0;
        List<PastPosition> Past = new List<PastPosition>();
        Vector2 Target = new Vector2(200, 200);
        public Vector2 FlowForce = Vector2.Zero;
        GameObject TargetObject = null;
        public bool IsSeeking = false;
        public RogueAgent(Texture2D inTexture) : base()
        {
            IsRogue = true;
            MaxForce = 15f;
            MaxSpeed = 250.0f;
            MaxForceSqared = MaxForce * MaxForce;
            MaxSpeedSquared = MaxSpeed * MaxSpeed;
            DilationDistanceSquared = DilationDistance * DilationDistance;
            WanderRadius = WanderDistance * 1.25f;
            Texture = inTexture;
        }
        public override void Update(float CurrentTime, float DeltaTime, float TimeModifier, List<Agent> Agents)
        {
            Acceleration += FlowForce;
            RogueSeek();
            Wander(CurrentTime, DeltaTime);
            CreateHistory();
            base.Update(CurrentTime, DeltaTime, TimeModifier, Agents);
        }

        private void RogueSeek()
        {            
            if(IsSeeking)
            {
                Target = TargetObject != null ? TargetObject.Position : Vector2.Zero;
                Vector2 seekVector = Seek(Target);
                seekVector.Normalize();
                seekVector *= SeekStrength;
                Acceleration += seekVector;
            }
        }

        private void CreateHistory()
        {    
            for (int i = Past.Count - 1; i >= 0; i--)
            {
                Past[i].Color.A -= 5;
                if (Past[i].Color.A < 5)
                {
                    Past.RemoveAt(i);
                }
            }
            if (Past.Count > 0)
            {
                int index = FlockTools.GetRandomInteger(Past.Count);
                Color PickedColor = FlockTools.Pick(new List<Color> { Color.DarkSeaGreen, Color.DarkTurquoise, Color.DarkRed, Color.LightYellow, Color.White, Color.FloralWhite }) * 0.5f;
                PickedColor.A = Past[index].Color.A;
                Past[index].Color = PickedColor;
            }
            Past.Add(new PastPosition() { Position = Position + FlockTools.GetRandomVector2(-2, 2, -2, 2), Color = Color.White });
        }

        void Wander(float CurrentTime, float DeltaTime)
        {
            Vector2 forward_target = new Vector2((float)Math.Cos(Orientation), (float)Math.Sin(Orientation));
            forward_target *= WanderDistance;

            WanderDelta = MathHelper.Clamp(WanderDelta + FlockTools.GetRandomFloat(-1, 1), -10, 10);
            float value = Noise.GetNoise(CurrentTime * WanderDelta * WanderRate, 0, 0) * WanderAmp;
            WanderTheta += MathHelper.WrapAngle(WanderTheta + value * DeltaTime);

            float x = WanderRadius * (float)Math.Cos(WanderTheta) - WanderRadius * (float)Math.Sin(WanderTheta);
            float y = WanderRadius * (float)Math.Cos(WanderTheta) + WanderRadius * (float)Math.Sin(WanderTheta);

            Vector2 wander_target = new Vector2(forward_target.X + x, forward_target.Y + y);
            wander_target.Normalize();
            Acceleration += wander_target * WanderStrength;
        }
        public override void Draw(SpriteBatch SB)
        {
            foreach(PastPosition p in Past)
            {
                SB.Draw(Texture, p.Position, p.Color);
            }
        }
    }
}
