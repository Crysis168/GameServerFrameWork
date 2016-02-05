using UnityEngine;
using System.Collections;

public static class NumberEncrypt 
{
	private static int _key = 0;
	private static int _keyTwo = 0;
	private static int _keyThree = 0;
	private static bool _bInit = false;
	// Use this for initialization
	public static int Encrypt(int value)
	{        
		if (_bInit == false)
		{
			_key = Random.Range (6000, 10000);
			_keyTwo = Random.Range (5, 2000);
			_keyThree = Random.Range (2, 5);
			_bInit = true;
		}
        return _key - _keyTwo + value * _keyThree;
	}

	public static int Decrypt(int value)
	{        
		if (_bInit == false)
		{
            _key = Random.Range(6000, 10000);
            _keyTwo = Random.Range(5, 2000);
            _keyThree = Random.Range(2, 5);
			_bInit = true;
		}
        return (value + _keyTwo - _key) / _keyThree;
	}
	
	public static float Encrypt(float value)
	{
		if (_bInit == false)
		{
            _key = Random.Range(6000, 10000);
            _keyTwo = Random.Range(5, 2000);
            _keyThree = Random.Range(2, 5);
			_bInit = true;
		}
		return _key -_keyTwo + value * _keyThree;
	}
	
	public static float Decrypt(float value)
	{
		if (_bInit == false)
		{
            _key = Random.Range(6000, 10000);
            _keyTwo = Random.Range(5, 2000);
            _keyThree = Random.Range(2, 5);
			_bInit = true;
		}
		return (value + _keyTwo-_key) / _keyThree;
	} 
}
