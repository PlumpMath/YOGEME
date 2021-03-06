/*
 * YOGEME.exe, All-in-one Mission Editor for the X-wing series, TIE through XWA
 * Copyright (C) 2007-2017 Michael Gaisser (mjgaisser@gmail.com)
 * Licensed under the MPL v2.0 or later
 * 
 * VERSION: 1.4
 */

/* CHANGELOG
 * v1.4, 171016
 * [ADD #11] form is now resizable, can be maximized
 * v1.2.3, 141214
 * [UPD] change to MPL
 * v1.2, 121006
 * - Settings passed in
 * v1.1.1, 120814
 * [FIX] MapData.Waypoints now based on BaseFlightGroup.BaseWaypoint, now updates back and forth with parent Form
 * - MapPaint() Orientation switch{} removed and condensed
 * - class renamed
 * v1.0, 110921
 * - Release
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Idmr.Common;

namespace Idmr.Yogeme
{
	/// <summary>graphical interface for craft waypoints</summary>
	public partial class MapForm : Form
	{
		int _zoom = 40;
		int w, h, mapX, mapY, mapZ;
		enum Orientation { XY, XZ, YZ };
		Orientation _displayMode = Orientation.XY;
		Bitmap _map;
		MapData[] _mapData;
		int[] _dragIcon = new int[2];	// [0] = fg, [1] = wp
		bool _loading = false;
		CheckBox[] chkWP = new CheckBox[22];
		Settings.Platform _platform;
		int _wpSetCount = 1;
		bool _isDragged;

		/// <param name="fg">TFlights array</param>
		public MapForm(Settings settings, Platform.Tie.FlightGroupCollection fg)
		{
			_platform = Settings.Platform.TIE;
			Import(fg);
			InitializeComponent();
			try { imgCraft.Images.AddStrip(Image.FromFile(Application.StartupPath + "\\images\\craft_TIE.bmp")); }
			catch(Exception x)
			{
				MessageBox.Show(x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
			}
			startup(settings);
		}

		/// <param name="fg">XFlights array</param>
		public MapForm(Settings settings, Platform.Xvt.FlightGroupCollection fg)
		{
			_platform = Settings.Platform.XvT;
			Import(fg);
			InitializeComponent();
			try { imgCraft.Images.AddStrip(Image.FromFile(Application.StartupPath + "\\images\\craft_XvT.bmp")); }
			catch (Exception x)
			{
				MessageBox.Show(x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
			}
			startup(settings);
		}

		/// <param name="fg">WFlights array</param>
		public MapForm(Settings settings, Platform.Xwa.FlightGroupCollection fg)
		{
			_platform = Settings.Platform.XWA;
			Import(fg);
			InitializeComponent();
			try { imgCraft.Images.AddStrip(Image.FromFile(Application.StartupPath + "\\images\\craft_XWA.bmp")); }
			catch (Exception x)
			{
				MessageBox.Show(x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
			}
			startup(settings);
		}

		/// <summary>Get the center pixel of pctMap and the coordinates it pertains to</summary>
		/// <returns>A point with the map coordinates in klicks</returns>
		PointF getCenterCoord()
		{
			PointF coord = new PointF();
			switch (_displayMode)
			{
				case Orientation.XY:
					coord.X = (w / 2 - mapX) / Convert.ToSingle(_zoom);
					coord.Y = (mapY - h / 2) / Convert.ToSingle(_zoom);
					break;
				case Orientation.XZ:
					coord.X = (w / 2 - mapX) / Convert.ToSingle(_zoom);
					coord.Y = (mapZ - h / 2) / Convert.ToSingle(_zoom);
					break;
				case Orientation.YZ:
					coord.X = (w / 2 - mapY) / Convert.ToSingle(_zoom);
					coord.Y = (mapZ - h / 2) / Convert.ToSingle(_zoom);
					break;
			}
			return coord;
		}

		/// <summary>Updaete mapX and mapY</summary>
		/// <param name="coord">The coordinate in klicks to use as the new center</param>
		void updateMapCoord(PointF coord)
		{
			switch (_displayMode)
			{
				case Orientation.XY:
					mapX = Convert.ToInt32(w / 2 - coord.X * Convert.ToSingle(_zoom));
					mapY = Convert.ToInt32(h / 2 + coord.Y * Convert.ToSingle(_zoom));
					break;
				case Orientation.XZ:
					mapX = Convert.ToInt32(w / 2 - coord.X * Convert.ToSingle(_zoom));
					mapZ = Convert.ToInt32(h / 2 + coord.Y * Convert.ToSingle(_zoom));
					break;
				case Orientation.YZ:
					mapY = Convert.ToInt32(w / 2 - coord.X * Convert.ToSingle(_zoom));
					mapZ = Convert.ToInt32(h / 2 + coord.Y * Convert.ToSingle(_zoom));
					break;
			}
			if (mapX / _zoom > 150) mapX = 150 * _zoom;
			if ((mapX - w) / _zoom < -150) mapX = -150 * _zoom + w;
			if (mapY / _zoom > 150) mapY = 150 * _zoom;
			if ((mapY - h) / _zoom < -150) mapY = -150 * _zoom + h;
			if (mapZ / _zoom > 150) mapZ = 150 * _zoom;
			if ((mapZ - h) / _zoom < -150) mapZ = -150 * _zoom + h;
		}

		/// <summary>Intialization routine, loads settings and config per platform</summary>
		void startup(Settings config)
		{
			#region checkbox array
			chkWP[0] = chkSP1;
			chkWP[1] = chkSP2;
			chkWP[2] = chkSP3;
			chkWP[3] = chkSP4;
			chkWP[4] = chkWP1;
			chkWP[5] = chkWP2;
			chkWP[6] = chkWP3;
			chkWP[7] = chkWP4;
			chkWP[8] = chkWP5;
			chkWP[9] = chkWP6;
			chkWP[10] = chkWP7;
			chkWP[11] = chkWP8;
			chkWP[12] = chkRDV;
			chkWP[13] = chkHYP;
			chkWP[14] = chkBRF;
			chkWP[15] = chkBRF2;
			chkWP[16] = chkBRF3;
			chkWP[17] = chkBRF4;
			chkWP[18] = chkBRF5;
			chkWP[19] = chkBRF6;
			chkWP[20] = chkBRF7;
			chkWP[21] = chkBRF8;
			for (int i = 0; i < 22; i++)
			{
				chkWP[i].CheckedChanged += new EventHandler(chkWPArr_CheckedChanged);
				chkWP[i].Tag = i;
			}
			#endregion
			updateLayout();
			mapX = w/2;
			mapY = h/2;
			mapZ = h/2;
			_dragIcon[0] = -1;
			_loading = true;
			chkTags.Checked = Convert.ToBoolean(config.MapOptions & Settings.MapOpts.FGTags);
			chkTrace.Checked = Convert.ToBoolean(config.MapOptions & Settings.MapOpts.Traces);
			int t = config.Waypoints;
			if (_platform==Settings.Platform.TIE)
			{
				for (int i=0;i<15;i++) chkWP[i].Checked = Convert.ToBoolean(t & (1 << i));
				for (int i=15;i<22;i++) chkWP[i].Enabled = false;
			}
			else if (_platform==Settings.Platform.XvT) for (int i=0;i<22;i++) chkWP[i].Checked = Convert.ToBoolean(t & (1 << i));
			else if (_platform==Settings.Platform.XWA)
			{
				for (int i=0;i<12;i++) chkWP[i].Checked = Convert.ToBoolean(t & (1 << i));
				chkWP[3].Text = "HYP";
				for (int i=12;i<22;i++) chkWP[i].Enabled = false;
				lblRegion.Visible = true;
				numRegion.Visible = true;
				lblOrder.Visible = true;
				numOrder.Visible = true;
			}
			this.MouseWheel += new MouseEventHandler(frmMap_MouseWheel);
			_loading = false;
		}

		/// <summary>Adjust control size/locations</summary>
		void updateLayout()
		{
			PointF center = getCenterCoord();
			pctMap.Width = Width - 120;
			pctMap.Height = Height - 155;
			w = pctMap.Width;
			h = pctMap.Height;
			_map = new Bitmap(w, h, PixelFormat.Format24bppRgb);
			lblCoor1.Top = Height - 59;
			lblCoor2.Top = lblCoor1.Top;
			lblZoom.Top = lblCoor1.Top;
			hscZoom.Top = lblCoor1.Top;
			hscZoom.Width = Width - 498;
			lblRegion.Left = Width - 268;
			numRegion.Left = Width - 218;
			lblOrder.Left = Width - 171;
			numOrder.Left = Width - 129;
			grpDir.Left = Width - 90;
			grpPoints.Left = grpDir.Left;
			chkTags.Left = grpDir.Left;
			chkTrace.Left = grpDir.Left;
			updateMapCoord(center);
			MapPaint(true);
		}

		/// <summary>The down-and-dirty function that handles map display </summary>
		/// <param name="persistant">When <b>true</b> draws to memory, <b>false</b> draws directly to the image</param>
		public void MapPaint(bool persistant)
		{
			if (_loading) return;
			#region orientation setup
			int X = mapX, Y = mapZ, coord1 = 0, coord2 = 2;
			switch (_displayMode)
			{
				case Orientation.XY:
					Y = mapY;
					coord2 = 1;
					break;
				case Orientation.YZ:
					X = mapY;
					coord1 = 1;
					break;
			}
			#endregion
			#region brush, pen and graphics setup
			// Create a new pen that we shall use for drawing the lines
			Pen pn = new Pen(Color.DarkRed);		
			SolidBrush sb = new SolidBrush(Color.Black);
			SolidBrush sbg = new SolidBrush(Color.DimGray);	// for FG tags
			Pen pnTrace = new Pen(Color.Gray);		// for WP traces
			Graphics g3;
			if (persistant) 
			{
				g3 = Graphics.FromImage(_map);		//graphics obj, load from the memory bitmap
				g3.Clear(SystemColors.Control);		//clear it
			}
			else 
			{
				g3 = pctMap.CreateGraphics();		//paint directly to pict
			}
			#endregion
			#region BG and grid
			g3.FillRectangle(sb, 0, 0, w, h);		//background
			for(int i = 0; i<200; i++)
			{
				g3.DrawLine(pn, 0, _zoom*i + Y, w, _zoom*i + Y);	//min lines, every klick
				g3.DrawLine(pn, 0, Y - _zoom*i, w, Y - _zoom*i);
				g3.DrawLine(pn, _zoom*i + X, 0, _zoom*i + X, h);
				g3.DrawLine(pn, X - _zoom*i, 0,X - _zoom*i, h);
			}
			pn.Width = 3;
			for(int i = 0; i<40; i++)
			{
				g3.DrawLine(pn, 0, _zoom*i*5 + Y, w, _zoom*i*5 + Y);	//maj lines, every 5 klicks
				g3.DrawLine(pn, 0, Y - _zoom*i*5, w, Y - _zoom*i*5);
				g3.DrawLine(pn, _zoom*i*5 + X, 0, _zoom*i*5 + X, h);
				g3.DrawLine(pn, X - _zoom*i*5, 0, X - _zoom*i*5, h);
			}
			pn.Color = Color.Red;
			pn.Width = 1;
			g3.DrawLine(pn, 0, Y, w, Y);	// origin lines
			g3.DrawLine(pn, X, 0, X, h);
			#endregion
			Bitmap bmptemp;
			byte[] bAdd = new byte[3];		// [0] = R, [1] = G, [2] = B
			for (int i = 0; i<_mapData.Length; i++)
			{
				#region IFF colors
				switch(_mapData[i].IFF)
				{
					case 0:
						pn.Color = Color.LimeGreen;		// FF32CD32
						break;
					case 1:
						pn.Color = Color.Crimson;		// FFDC143C
						break;
					case 2:
						pn.Color = Color.RoyalBlue;		// FF4169E1
						break;
					case 3:
						if (_platform == Settings.Platform.TIE) pn.Color = Color.DarkOrchid;		// FF9932CC
						else pn.Color = Color.Yellow;	// FFFFFF00
						break;
					case 4:
						pn.Color = Color.Red;			// FFFF0000
						break;
					case 5:
						if (_platform == Settings.Platform.TIE) pn.Color = Color.Fuchsia;			// FFFF00FF
						else pn.Color = Color.DarkOrchid;	// FF9932CC
						break;
				}
				bAdd[0] = pn.Color.R;
				bAdd[1] = pn.Color.G;
				bAdd[2] = pn.Color.B;
				#endregion
				bmptemp = new Bitmap(imgCraft.Images[_mapData[i].Craft]);
				bmptemp = mask(bmptemp, bAdd);
				// work through each WP and determine if it needs to be displayed, then place it on the map
				// draw tags if required
				// if previous sequential WP is checked and trace is required, draw trace line according to WP type
				for (int k = 0; k < 4; k++)	// Start
				{
					if (chkWP[k].Checked && _mapData[i].WPs[0][k].Enabled && (_platform == Settings.Platform.XWA ? _mapData[i].WPs[0][k][4] == (short)(numRegion.Value - 1) : true))
					{
						g3.DrawImageUnscaled(bmptemp, _zoom * _mapData[i].WPs[0][k][coord1] / 160 + X - 8, -_zoom * _mapData[i].WPs[0][k][coord2] / 160 + Y - 8);
						if (chkTags.Checked) g3.DrawString(_mapData[i].Name + " " + chkWP[k].Text, MapForm.DefaultFont, sbg, _zoom * _mapData[i].WPs[0][k][coord1] / 160 + X + 8, -_zoom * _mapData[i].WPs[0][k][coord2] / 160 + Y + 8);
					}
				}
				if (_platform == Settings.Platform.XWA)	// WPs
				{
					int ord = (int)((numRegion.Value - 1) * 4 + (numOrder.Value - 1) + 1);
					for (int k = 0; k < 8; k++)
					{
						if (chkWP[k + 4].Checked && _mapData[i].WPs[ord][k].Enabled)
						{
							g3.DrawEllipse(pn, _zoom * _mapData[i].WPs[ord][k][coord1] / 160 + X - 1, -_zoom * _mapData[i].WPs[ord][k][coord2] / 160 + Y - 1, 3, 3);
							if (chkTags.Checked) g3.DrawString(_mapData[i].Name + " " + chkWP[k + 4].Text, MapForm.DefaultFont, sbg, _zoom * _mapData[i].WPs[ord][k][coord1] / 160 + X + 4, -_zoom * _mapData[i].WPs[ord][k][coord2] / 160 + Y + 4);
							if (chkTrace.Checked && k == 0 && _mapData[i].WPs[0][0][4] == (numRegion.Value - 1) && chkWP[0].Checked) g3.DrawLine(pnTrace, _zoom * _mapData[i].WPs[0][0][coord1] / 160 + X, -_zoom * _mapData[i].WPs[0][0][coord2] / 160 + Y, _zoom * _mapData[i].WPs[ord][k][coord1] / 160 + X, -_zoom * _mapData[i].WPs[ord][k][coord2] / 160 + Y);
							else if (chkTrace.Checked && chkWP[k + 3].Checked) g3.DrawLine(pnTrace, _zoom * _mapData[i].WPs[ord][k - 1][coord1] / 160 + X, -_zoom * _mapData[i].WPs[ord][k - 1][coord2] / 160 + Y, _zoom * _mapData[i].WPs[ord][k][coord1] / 160 + X, -_zoom * _mapData[i].WPs[ord][k][coord2] / 160 + Y);
						}
					}
					continue;
				}
				else
				{
					for (int k = 4; k < 12; k++)
					{
						if (chkWP[k].Checked && _mapData[i].WPs[0][k].Enabled)
						{
							g3.DrawEllipse(pn, _zoom * _mapData[i].WPs[0][k][coord1] / 160 + X - 1, -_zoom * _mapData[i].WPs[0][k][coord2] / 160 + Y - 1, 3, 3);
							if (chkTags.Checked) g3.DrawString(_mapData[i].Name + " " + chkWP[k].Text, MapForm.DefaultFont, sbg, _zoom * _mapData[i].WPs[0][k][coord1] / 160 + X + 4, -_zoom * _mapData[i].WPs[0][k][coord2] / 160 + Y + 4);
							if (chkWP[(k == 4 ? 0 : (k - 1))].Checked && chkTrace.Checked) g3.DrawLine(pnTrace, _zoom * _mapData[i].WPs[0][(k == 4 ? 0 : (k - 1))][coord1] / 160 + X, -_zoom * _mapData[i].WPs[0][(k == 4 ? 0 : (k - 1))][coord2] / 160 + Y, _zoom * _mapData[i].WPs[0][k][coord1] / 160 + X, -_zoom * _mapData[i].WPs[0][k][coord2] / 160 + Y);
						}
					}
				}
				// remaining are not valid for XWA
				if (chkWP[12].Checked && _mapData[i].WPs[0][12].Enabled) // RND
				{
					g3.DrawEllipse(pn, _zoom * _mapData[i].WPs[0][12][coord1] / 160 + X - 1, -_zoom * _mapData[i].WPs[0][12][coord2] / 160 + Y - 1, 3, 3);
					if (chkTags.Checked) g3.DrawString(_mapData[i].Name + " " + chkWP[12].Text, MapForm.DefaultFont, sbg, _zoom * _mapData[i].WPs[0][12][coord1] / 160 + X + 4, -_zoom * _mapData[i].WPs[0][12][coord2] / 160 + Y + 4);
				}
				if (chkWP[13].Checked && _mapData[i].WPs[0][13].Enabled)	// HYP
				{
					g3.DrawEllipse(pn, _zoom * _mapData[i].WPs[0][13][coord1] / 160 + X - 1, -_zoom * _mapData[i].WPs[0][13][coord2] / 160 + Y - 1, 3, 3);
					if (chkTags.Checked) g3.DrawString(_mapData[i].Name + " " + chkWP[13].Text, MapForm.DefaultFont, sbg, _zoom * _mapData[i].WPs[0][13][coord1] / 160 + X + 4, -_zoom * _mapData[i].WPs[0][13][coord2] / 160 + Y + 4);
					if (chkTrace.Checked)
					{
						// in this case, make sure last visible WP is the last enabled before tracing to HYP
						pnTrace.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
						for (int k = 4; k < 12; k++)
						{
							if (k != 11)
							{
								if (chkWP[k].Checked && _mapData[i].WPs[0][k].Enabled && !_mapData[i].WPs[0][k + 1].Enabled)
								{
									g3.DrawLine(pnTrace, _zoom * _mapData[i].WPs[0][k][coord1] / 160 + X, -_zoom * _mapData[i].WPs[0][k][coord2] / 160 + Y, _zoom * _mapData[i].WPs[0][13][coord1] / 160 + X, -_zoom * _mapData[i].WPs[0][13][coord2] / 160 + Y);
									break;
								}
							}
							else if (chkWP[k].Checked && _mapData[i].WPs[0][k].Enabled) g3.DrawLine(pnTrace, _zoom * _mapData[i].WPs[0][11][coord1] / 160 + X, -_zoom * _mapData[i].WPs[0][11][coord2] / 160 + Y, _zoom * _mapData[i].WPs[0][13][coord1] / 160 + X, -_zoom * _mapData[i].WPs[0][13][coord2] / 160 + Y); ;
						}
						pnTrace.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
					}
				}
				for (int k = 14; k < 22; k++)	// BRF
				{
					if (chkWP[k].Checked && _mapData[i].WPs[0][k].Enabled)
					{
						g3.DrawImageUnscaled(bmptemp, _zoom * _mapData[i].WPs[0][k][coord1] / 160 + X - 8, -_zoom * _mapData[i].WPs[0][k][coord2] / 160 + Y - 8);
						if (chkTags.Checked) g3.DrawString(_mapData[i].Name + " " + chkWP[k].Text, MapForm.DefaultFont, sbg, _zoom * _mapData[i].WPs[0][k][coord1] / 160 + X + 8, -_zoom * _mapData[i].WPs[0][k][coord2] / 160 + Y + 8);
					}
				}
			}
			if (persistant) pctMap.Invalidate();		// since it's drawing to memory, this refreshes the pct.  Removes the flicker when zooming
			g3.Dispose();
		}

		/// <summary>Loads FG data into the MapData class the form uses</summary>
		/// <param name="fg">TFlights array</param>
		public void Import(Platform.Tie.FlightGroupCollection fg)
		{
			int numCraft = fg.Count;
			_mapData = new MapData[numCraft];
			for (int i=0;i<numCraft;i++)
			{
				_mapData[i] = new MapData(_platform);
				_mapData[i].Craft = fg[i].CraftType;
				_mapData[i].WPs[0] = fg[i].Waypoints;
				_mapData[i].IFF = fg[i].IFF;
				_mapData[i].Name = fg[i].Name;
			}
		}

		/// <summary>Loads FG data into the MapData class the form uses</summary>
		/// <param name="fg">XFlights array</param>
		public void Import(Platform.Xvt.FlightGroupCollection fg)
		{
			int numCraft = fg.Count;
			_mapData = new MapData[numCraft];
			for (int i=0;i<numCraft;i++)
			{
				_mapData[i] = new MapData(_platform);
				_mapData[i].Craft = fg[i].CraftType;
				_mapData[i].WPs[0] = fg[i].Waypoints;
				_mapData[i].IFF = fg[i].IFF;
				_mapData[i].Name = fg[i].Name;
			}
		}

		/// <summary>Loads FG data into the MapData class the form uses</summary>
		/// <param name="fg">WFlights array</param>
		public void Import(Platform.Xwa.FlightGroupCollection fg)
		{
			int numCraft = fg.Count;
			_mapData = new MapData[numCraft];
			_wpSetCount = 17;
			for (int i=0;i<numCraft;i++)
			{
				_mapData[i] = new MapData(_platform);
				_mapData[i].Craft = fg[i].CraftType;
				_mapData[i].WPs[0] = fg[i].Waypoints;
				for (int j = 0; j < 16; j++)
				{
					int region = j / 4;
					int order = j % 4;
					_mapData[i].WPs[j + 1] = fg[i].Orders[region, order].Waypoints;
				}
				_mapData[i].IFF = fg[i].IFF;
				_mapData[i].Name = fg[i].Name;
			}
		}

		/// <summary>Change the zoom of the map and reset local x/y/z coords as neccessary</summary>
		void hscZoom_ValueChanged(object sender, EventArgs e)
		{
			PointF center = getCenterCoord();
			_zoom = hscZoom.Value;
			updateMapCoord(center);
			MapPaint(true);
			lblZoom.Text = "Zoom: " + _zoom.ToString();
		}

		/// <summary>Rotate map to Top view</summary>
		void optXY_CheckedChanged(object sender, EventArgs e)
		{
			if (optXY.Checked)
			{
				_displayMode = Orientation.XY;
				lblCoor1.Text = "X:";
				lblCoor2.Text = "Y:";
				MapPaint(false);
			}
		}

		/// <summary>Rotate map to Front view</summary>
		void optXZ_CheckedChanged(object sender, EventArgs e)
		{
			if (optXZ.Checked)
			{
				_displayMode = Orientation.XZ;
				lblCoor1.Text = "X:";
				lblCoor2.Text = "Z:";
				MapPaint(false);
			}
		}

		/// <summary>Rotate map to Side view </summary>
		void optYZ_CheckedChanged(object sender, EventArgs e)
		{
			if (optYZ.Checked)
			{
				mapY = w/2 - mapY + h/2;
				_displayMode = Orientation.YZ;
				lblCoor1.Text = "Y:";
				lblCoor2.Text = "Z:";
				MapPaint(false);
			}
			else mapY = w/2 + h/2 - mapY;
		}

		#region pctMap
		void pctMap_DoubleClick(object sender, EventArgs e)
		{
			// zoom in
			if (hscZoom.Value == 500) return;
			mapX += (mapX - w/2);
			mapY += (mapY - h/2);
			mapZ += (mapZ - w/2);
			if (mapX/_zoom > 150) mapX = 150*_zoom;
			if ((mapX-w)/_zoom < -150) mapX = -150*_zoom + w;
			if (mapY/_zoom > 150) mapY = 150*_zoom;
			if ((mapY-h)/_zoom < -150) mapY = -150*_zoom + h;
			if (mapZ/_zoom > 150) mapZ = 150*_zoom;
			if ((mapZ-h)/_zoom < -150) mapZ = -150*_zoom + h;
			hscZoom.Value = (_zoom < 250 ? _zoom * 2 : 500);
		}
		void pctMap_MouseDown(object sender, MouseEventArgs e)
		{
			// move map, center on mouse
			if (e.Button.ToString() == "Right")
			{
				switch (_displayMode)
				{
					#region Mode check
					case Orientation.XY:
						mapX += w / 2 - e.X;
						mapY += h / 2 - e.Y;
						if (mapX / _zoom > 150) mapX = 150 * _zoom;
						if ((mapX - w) / _zoom < -150) mapX = -150 * _zoom + w;
						if (mapY / _zoom > 150) mapY = 150 * _zoom;
						if ((mapY - h) / _zoom < -150) mapY = -150 * _zoom + h;
						break;
					case Orientation.XZ:
						mapX += w / 2 - e.X;
						mapZ += h / 2 - e.Y;
						if (mapX / _zoom > 150) mapX = 150 * _zoom;
						if ((mapX - w) / _zoom < -150) mapX = -150 * _zoom + w;
						if (mapZ / _zoom > 150) mapZ = 150 * _zoom;
						if ((mapZ - h) / _zoom < -150) mapZ = -150 * _zoom + h;
						break;
					case Orientation.YZ:
						mapY += w / 2 - e.X;
						mapZ += h / 2 - e.Y;
						if (mapY / _zoom > 150) mapY = 150 * _zoom;
						if ((mapY - w) / _zoom < -150) mapY = -150 * _zoom + w;
						if (mapZ / _zoom > 150) mapZ = 150 * _zoom;
						if ((mapZ - h) / _zoom < -150) mapZ = -150 * _zoom + h;
						break;
					#endregion
				}
				MapPaint(false);
			}
			else if (e.Button.ToString() == "Left")
			{
				// Okay, for every flightgroup, check every waypoint
				// if it's enabled, check to see if the center is about where the mouse is
				// store FG and WP indexes
				for (int fg = 0; fg < _mapData.Length; fg++)
				{
					for (int wpSet = 0; wpSet < _wpSetCount; wpSet++)
					{
						for (int wp = 0; wp < _mapData[fg].WPs[wpSet].Length; wp++)
						{
							if (!chkWP[(wpSet != 0 ? wp + 4 : wp)].Checked || !_mapData[fg].WPs[wpSet][wp].Enabled) continue;
							if ((_displayMode == Orientation.XY && isApprox(_zoom * _mapData[fg].WPs[wpSet][wp].RawX / 160 + mapX, e.X) && isApprox(-_zoom * _mapData[fg].WPs[wpSet][wp].RawY / 160 + mapY, e.Y)) || (_displayMode == Orientation.XZ && isApprox(_zoom * _mapData[fg].WPs[wpSet][wp].RawX / 160 + mapX, e.X) && isApprox(-_zoom * _mapData[fg].WPs[wpSet][wp].RawZ / 160 + mapZ, e.Y)) || (_displayMode == Orientation.YZ && isApprox(_zoom * _mapData[fg].WPs[wpSet][wp].RawY / 160 + mapY, e.X) && isApprox(-_zoom * _mapData[fg].WPs[wpSet][wp].RawZ / 160 + mapZ, e.Y)))
							{
								_dragIcon[0] = fg;
								_dragIcon[1] = wp + (wpSet != 0 ? wpSet * 8 - 4 : 0);
								break;
							}
						}
					}
				}
			}
			else if (e.Button.ToString() == "Middle")
			{
				mapX = w/2;
				mapY = h/2;
				mapZ = h/2;
				hscZoom.Value = 40;
				MapPaint(false);
			}
		}
		void pctMap_MouseEnter(object sender, EventArgs e) { pctMap.Focus(); }
		void pctMap_MouseMove(object sender, MouseEventArgs e)
		{
			// gets the current mouse location in klicks
			double msX, msY;
			switch (_displayMode)
			{
				case Orientation.XY:
					msX = (e.X - mapX) / Convert.ToDouble(_zoom);
					msY = (mapY - e.Y) / Convert.ToDouble(_zoom);
					lblCoor1.Text = "X: " + Math.Round(msX,2).ToString();
					lblCoor2.Text = "Y: " + Math.Round(msY,2).ToString();
					break;
				case Orientation.XZ:
					msX = (e.X - mapX) / Convert.ToDouble(_zoom);
					msY = (mapZ - e.Y) / Convert.ToDouble(_zoom);
					lblCoor1.Text = "X: " + Math.Round(msX,2).ToString();
					lblCoor2.Text = "Z: " + Math.Round(msY,2).ToString();
					break;
				case Orientation.YZ:
					msX = (e.X - mapY) / Convert.ToDouble(_zoom);
					msY = (mapZ - e.Y) / Convert.ToDouble(_zoom);
					lblCoor1.Text = "Y: " + Math.Round(msX,2).ToString();
					lblCoor2.Text = "Z: " + Math.Round(msY,2).ToString();
					break;
			}
		}
		void pctMap_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button.ToString() == "Left" && _dragIcon[0] != -1)
			{
				// WP was dragged, reassign to the new location and repaint
				int wpSet = 0;
				if (_platform == Settings.Platform.XWA && _dragIcon[1] >= 4)
				{
					wpSet = (_dragIcon[1] + 4) / 8;
					_dragIcon[1] = (_dragIcon[1] - 4) % 8;
				}
				switch(_displayMode)
				{
					case Orientation.XY:
						_mapData[_dragIcon[0]].WPs[wpSet][_dragIcon[1]].RawX = (short)((e.X - mapX) / Convert.ToDouble(_zoom) * 160);
						_mapData[_dragIcon[0]].WPs[wpSet][_dragIcon[1]].RawY = (short)((mapY - e.Y) / Convert.ToDouble(_zoom) * 160);
						break;
					case Orientation.XZ:
						_mapData[_dragIcon[0]].WPs[wpSet][_dragIcon[1]].RawX = (short)((e.X - mapX) / Convert.ToDouble(_zoom) * 160);
						_mapData[_dragIcon[0]].WPs[wpSet][_dragIcon[1]].RawZ = (short)((mapZ - e.Y) / Convert.ToDouble(_zoom) * 160);
						break;
					case Orientation.YZ:
						_mapData[_dragIcon[0]].WPs[wpSet][_dragIcon[1]].RawY = (short)((e.X - mapY) / Convert.ToDouble(_zoom) * 160);
						_mapData[_dragIcon[0]].WPs[wpSet][_dragIcon[1]].RawZ = (short)((mapZ - e.Y) / Convert.ToDouble(_zoom) * 160);
						break;
				}
				MapPaint(true);
				_dragIcon[0] = -1;
			}
		}
		void pctMap_Paint(object sender, PaintEventArgs e)
		{
			Graphics objGraphics;
			//You can't modify e.Graphics directly.
			objGraphics = e.Graphics;
			// Draw the contents of the bitmap on the form.
			objGraphics.DrawImage(_map, 0, 0, _map.Width, _map.Height);
		}

		/// <summary>Used to determine if mouse click is near a craft waypoint</summary>
		/// <returns>True if num1==(num2 � 6)</returns>
		bool isApprox(int num1, double num2)
		{
			// +/- 6 is a good enough size
			if (num1 <= (num2+6) && num1 >= (num2-6)) return true;
			else return false;
		}
		#endregion
		#region frmMap
		void frmMap_Activated(object sender, EventArgs e) { MapPaint(true); }
		void frmMap_Closed(object sender, EventArgs e) { _map.Dispose(); }
		void frmMap_Load(object sender, EventArgs e)
		{
			_map = new Bitmap(w, h, PixelFormat.Format24bppRgb);
			MapPaint(true);
		}
		void frmMap_MouseWheel(object sender, MouseEventArgs e)
		{
			if (hscZoom.Value < 25 && e.Delta < 0) hscZoom.Value = 5;
			else if (hscZoom.Value > 480 && e.Delta > 0) hscZoom.Value = 500;
			else hscZoom.Value += 20 * Math.Sign(e.Delta);
		}
		void MapForm_Resize(object sender, EventArgs e)
		{
			if (!_isDragged) updateLayout();
		}
		void MapForm_ResizeBegin(object sender, EventArgs e)
		{
			_isDragged = true;
		}
		void MapForm_ResizeEnd(object sender, EventArgs e)
		{
			_isDragged = false;
			updateLayout();
		}
		#endregion

		/// <summary>Take the original image from the craft image strip and adds the RGB values from the craft IFF</summary>
		/// <param name="craftImage">The greyscale craft image</param>
		/// <param name="iff">An array containing the RGB values as per the craft IFF</param>
		/// <returns>Colorized craft image according to IFF</returns>
		Bitmap mask(Bitmap craftImage, byte[] iff)
		{
			// craftImage comes in as 32bppRGB, but we force the image into 24bppRGB with LockBits
			BitmapData bmData = GraphicsFunctions.GetBitmapData(craftImage, PixelFormat.Format24bppRgb);
			byte[] pix = new byte[bmData.Stride*bmData.Height];
			GraphicsFunctions.CopyImageToBytes(bmData, pix);
			for(int y = 0;y < craftImage.Height; y++)
			{
				for(int x = 0, pos = bmData.Stride*y;x < craftImage.Width; x++)
				{
					// stupid thing returns BGR instead of RGB
					pix[pos+x*3] = (byte)(pix[pos+x*3] * iff[2] / 255);		// get intensity, apply to IFF mask
					pix[pos+x*3+1] = (byte)(pix[pos+x*3+1] * iff[1] / 255);
					pix[pos+x*3+2] = (byte)(pix[pos+x*3+2] * iff[0] / 255);
				}
			}
			GraphicsFunctions.CopyBytesToImage(pix, bmData);
			craftImage.UnlockBits(bmData);
			craftImage.MakeTransparent(Color.Black);
			return craftImage;
		}

		#region Checkboxes
		void chkTags_CheckedChanged(object sender, EventArgs e) { if (!_loading) MapPaint(true); }
		void chkTrace_CheckedChanged(object sender, EventArgs e) { if (!_loading) MapPaint(true); }
		void chkWPArr_CheckedChanged(object sender, EventArgs e)
		{
			if (_loading) return;
			if ((CheckBox)sender == chkWP[14] && chkWP[14].Checked) for (int i=0;i<14;i++) chkWP[i].Checked = false;
			MapPaint(true);
		}
		#endregion

		void numOrder_ValueChanged(object sender, EventArgs e) { if (!_loading) MapPaint(true); }
		void numRegion_ValueChanged(object sender, EventArgs e) { if (!_loading) MapPaint(true); }

		struct MapData
		{
			public MapData(Settings.Platform platform)
			{
				Craft = 0;
				IFF = 0;
				Name = "";
				WPs = null;
				switch (platform)
				{
					case Settings.Platform.TIE:
						WPs = new Platform.Tie.FlightGroup.Waypoint[1][];
						//WPs[0] = new Platform.Tie.FlightGroup.Waypoint[15];
						//for(int i = 0; i < WPs[0].Length; i++) WPs[0][i] = new Platform.Tie.FlightGroup.Waypoint();
						break;
					case Settings.Platform.XvT:
						WPs = new Platform.Xvt.FlightGroup.Waypoint[1][];
						//WPs[0] = new Platform.Xvt.FlightGroup.Waypoint[22];
						//for (int i = 0; i < WPs[0].Length; i++) WPs[0][i] = new Platform.Xvt.FlightGroup.Waypoint();
						break;
					case Settings.Platform.XWA:
						WPs = new Platform.Xwa.FlightGroup.Waypoint[17][];
						//for (int i = 0; i < x; i++) WPs[i] = new Platform.Xwa.FlightGroup.Waypoint();
						break;
				}
			}

			public int Craft;
			public byte IFF;
			public string Name;

			public Platform.BaseFlightGroup.BaseWaypoint[][] WPs;
		}
	}
}