using System;
using System.Collections.Generic;
using FlaxEngine;

namespace GridSystem;

/// <summary>
/// GridDebugObject Script.
/// </summary>
public class GridDebugObject : Script
{
	public TextRender TextRender;
	public object GridObject { get; protected set; }

	public virtual void SetGridObject(object gridObject)
	{
		GridObject = gridObject;
		SetText(GridObject.ToString());
	}
	protected virtual void SetText(string text)
	{
		TextRender.Text = text;
	}


}
