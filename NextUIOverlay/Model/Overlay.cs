﻿using System;
using System.Drawing;
using System.Reactive.Subjects;
using NextUIOverlay.Data.Input;
namespace NextUIOverlay.Model;

[Serializable]
	public class Overlay {
		public Guid Guid { get; set; }
		public readonly object renderLock = new();

		protected string name = "New Overlay";
		protected string url = "";
		protected Size size;
		protected bool fullscreen;
		protected Point position;

		public string Name {
			get { return name; }
			set {
				if (name == value) {
					return;
				}

				name = value;
				NameChange?.Invoke(this, value);
			}
		}

		public string Url {
			get { return url; }
			set {
				if (url == value) {
					return;
				}

				url = value;
				UrlChange?.Invoke(this, url);
			}
		}

		public Size Size {
			get { return FullScreen ? FullScreenSize : size; }
			set {
				if (size.Equals(value)) {
					return;
				}

				var s = new Size(value.Width, value.Height);
				if (s.Width < 1) {
					s.Width = 1;
				}

				if (s.Height < 1) {
					s.Height = 1;
				}

				size = s;
				SizeChange.OnNext(s);
			}
		}

		public Point Position {
			get { return FullScreen ? Point.Empty : position; }
			set {
				if (position == value) {
					return;
				}

				position = value;
				PositionChange?.Invoke(this, position);
			}
		}

		public bool FullScreen {
			get { return fullscreen; }
			set {
				if (fullscreen == value) {
					return;
				}

				fullscreen = value;
				SizeChange.OnNext(FullScreenSize);
			}
		}

		public bool ClickThrough { get; set; }
		public bool TypeThrough { get; set; }
		public bool Locked { get; set; } = true;
		public bool Hidden { get; set; }
		public bool Toggled { get; set; }
		public Size FullScreenSize { get; set; }
		public OverlayVisibility VisibilityShow { get; set; }
		public OverlayVisibility VisibilityHide { get; set; }
		public bool Resizing { get; set; }

		// ReSharper disable InconsistentNaming
		public event EventHandler<string>? NameChange;
		public event EventHandler<string>? UrlChange;
		public event EventHandler<Point>? PositionChange;

		// public event EventHandler<Cursor>? CursorChange;

		// public event EventHandler<MouseMoveEventRequest>? MouseMoveEvent;
		// public event EventHandler<MouseClickEventRequest>? MouseClickEvent;
		// public event EventHandler<MouseLeaveEventRequest>? MouseLeaveEvent;
		// public event EventHandler<MouseWheelEventRequest>? MouseWheelEvent;
		//
		// public event EventHandler<KeyEventRequest>? KeyEvent;
		// public event EventHandler<PaintRequest>? Paint;
		// public event EventHandler<PopupSizeRequest>? PopupSize;
		// public event EventHandler<bool>? PopupShow;

		// This thing needs special handling
		public Subject<Size> SizeChange = new();
		// ReSharper restore InconsistentNaming

		// public event Action? DebugRequest;
		// public event Action? ReloadRequest;
		// public event Action? DisposeRequest;
		// public event EventHandler? Remove;
		// public event Action? BrowserDispose;

		public Overlay(string url, Size newSize) {
			Guid = Guid.NewGuid();
			Url = url;
			size = newSize;
			if (size.Width != 0 && size.Height != 0) {
				return;
			}

			size.Width = 800;
			size.Height = 600;
		}

		public void Navigate(string newUrl) {
			Url = newUrl;
		}

		// public void Reload() {
		// 	ReloadRequest?.Invoke();
		// }
		//
		// public void Debug() {
		// 	DebugRequest?.Invoke();
		// }
		//
		// public void Dispose() {
		// 	DisposeRequest?.Invoke();
		// }
		//
		// public void RemoveRequest() {
		// 	Remove?.Invoke(this, EventArgs.Empty);
		// }
		//
		// public void BrowserDisposeRequest() {
		// 	BrowserDispose?.Invoke();
		// }

		// public void SetCursor(Cursor newCursor) {
		// 	Cursor = newCursor;
		// }

		// public void RequestMouseMoveEvent(MouseMoveEventRequest request) {
		// 	MouseMoveEvent?.Invoke(this, request);
		// }
		//
		// public void RequestMouseClickEvent(MouseClickEventRequest request) {
		// 	MouseClickEvent?.Invoke(this, request);
		// }
		//
		// public void RequestMouseLeaveEvent(MouseLeaveEventRequest request) {
		// 	MouseLeaveEvent?.Invoke(this, request);
		// }
		//
		// public void RequestMouseWheelEvent(MouseWheelEventRequest request) {
		// 	MouseWheelEvent?.Invoke(this, request);
		// }
		//
		// public void RequestKeyEvent(KeyEventRequest request) {
		// 	KeyEvent?.Invoke(this, request);
		// }
		//
		// public void PaintRequest(PaintRequest paintRequest) {
		// 	Paint?.Invoke(this, paintRequest);
		// }
		//
		// public void ShowPopup(bool show) {
		// 	PopupShow?.Invoke(this, show);
		// }
		//
		// public void PopupSizeChange(PopupSizeRequest popupSizeRequest) {
		// 	PopupSize?.Invoke(this, popupSizeRequest);
		// }
	}
