using UnityEngine;
using System.Collections;

public class SkyControl2142 : MonoBehaviour
{
	public Material sky;
	public Vector2 scrollDirection;
	public float accelerationX, accelerationY;
	private float speedX, speedY;
  public Vector2 scrollDirection1;
	public Vector2 scrollDirection2;
	private Vector2 offset;
	public float alphaSpeed;
	private float alpha;
	public float rotation;
	private Transform playerPosition;

	void Start()
	{
		scrollDirection = scrollDirection1;
		offset = new Vector2();
		alpha = 0;
		playerPosition = GameObject.FindGameObjectWithTag("Player").transform;
	}

	void Update()
	{
		offset += scrollDirection * Time.deltaTime;
		offset.x = offset.x % 1;
		offset.y = offset.y % 1;
		Vector2 texOffset = sky.GetTextureOffset("_FrontTexture");
		sky.SetTextureOffset("_FrontTexture", offset);
		sky.SetFloat("_Altitude", playerPosition.position.y / 4000);

		if(alphaSpeed > 0)
		{
			alpha += Time.deltaTime * alphaSpeed;
			if(alpha > 0.8f)
			{
				alpha = 0.8f;
				alphaSpeed = -alphaSpeed;
			}
		}
		else
		{
			alpha += Time.deltaTime * alphaSpeed;
			if(alpha < -1f)
			{
				alpha = -1f;
				alphaSpeed = -alphaSpeed;
			}
		}
		sky.SetFloat("_AlphaCutoff", alpha);


		if(accelerationX > 0)
		{
			speedX += Time.deltaTime * accelerationX;
			if(speedX > 1)
			{
				speedX = 1;
				accelerationX = -accelerationX;
			}
		}
		else
		{
			speedX += Time.deltaTime * accelerationX;
			if(speedX < 0)
			{
				speedX = 0;
				accelerationX = -accelerationX;
			}
		}

		if(accelerationY > 0)
		{
			speedY += Time.deltaTime * accelerationY;
			if(speedY > 1)
			{
				speedY = 1;
				accelerationY = -accelerationY;
			}
		}
		else
		{
			speedY += Time.deltaTime * accelerationY;
			if(speedY < 0)
			{
				speedY = 0;
				accelerationY = -accelerationY;
			}
		}

		scrollDirection.x = Mathf.SmoothStep(scrollDirection1.x, scrollDirection2.x, speedX);
		scrollDirection.y = Mathf.SmoothStep(scrollDirection1.y, scrollDirection2.y, speedY);
	}
}
