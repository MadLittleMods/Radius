using UnityEngine;

public class CharacterState
{
	public Vector3 Position = Vector3.zero;
	
	public Vector3 Velocity = Vector3.zero;
	
	// Like input added every frame
	// This is just a nice variable to see if the character moved
	public Vector3 InstantVelocity = Vector3.zero;
	
	public CharacterState()
	{
		
	}
	
	public CharacterState(CharacterState s)
	{
		this.Position = s.Position;
		this.Velocity = s.Velocity;
		this.InstantVelocity = s.InstantVelocity;
	}
	
	public CharacterState(Vector3 position, Vector3 velocity)
	{
		this.Position = position;
		
		this.Velocity = velocity;
	}
	
	public static CharacterState Lerp(CharacterState from, CharacterState to, float t)
	{
		return new CharacterState(Vector3.Lerp(from.Position, to.Position, t), Vector3.Lerp(from.Velocity, to.Velocity, t));
	}
	
	public static implicit operator string(CharacterState s)
	{
		return s.ToString();
	}
	
	
	public override string ToString()
	{
		return "p: " + this.Position + " v: " + this.Velocity;
	}
}