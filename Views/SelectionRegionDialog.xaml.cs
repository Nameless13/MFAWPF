﻿using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MFAWPF.Utils;
using MFAWPF.ViewModels;
using HandyControl.Controls;
using HandyControl.Data;
using MFAWPF.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Attribute = MFAWPF.Utils.Attribute;

namespace MFAWPF.Views;

public partial class SelectionRegionDialog : CustomWindow
{
    public string ImgPath { get; set; }
    private Point _startPoint;
    private Rectangle _selectionRectangle;
    public List<int> Output { get; set; }
    public bool IsRoi { get; set; }

    public SelectionRegionDialog(BitmapImage bitmapImage) :
        base()
    {
        InitializeComponent();
        UpdateImage(bitmapImage);
    }

    private double _scaleRatio;
    private double originWidth;
    private double originHeight;
    private void UpdateImage(BitmapImage _imageSource)
    {
        image.Source = _imageSource;
        Console.WriteLine($"{_imageSource.PixelWidth},{_imageSource.PixelHeight}");

        originWidth = _imageSource.PixelWidth;
        originHeight = _imageSource.PixelHeight;

        double maxWidth = image.MaxWidth;
        double maxHeight = image.MaxHeight;

        double widthRatio = maxWidth / originWidth;
        double heightRatio = maxHeight / originHeight;
        _scaleRatio = Math.Min(widthRatio, heightRatio);

        image.Width = originWidth * _scaleRatio;
        image.Height = originHeight * _scaleRatio;

        SelectionCanvas.Width = image.Width;
        SelectionCanvas.Height = image.Height;
        Width = image.Width + 20;
        Height = image.Height + 100;
    }

     private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(image);
        var canvasPosition = e.GetPosition(SelectionCanvas);

        // 判断点击是否在Image边缘5个像素内
        if (canvasPosition.X < image.ActualWidth + 5 && canvasPosition.Y < image.ActualHeight + 5 &&
            canvasPosition.X > -5 && canvasPosition.Y > -5)
        {
            if (_selectionRectangle != null)
            {
                SelectionCanvas.Children.Remove(_selectionRectangle);
            }

            // 如果超出5个像素内，调整点击位置到Image边界
            if (position.X < 0) position.X = 0;
            if (position.Y < 0) position.Y = 0;
            if (position.X > image.ActualWidth) position.X = image.ActualWidth;
            if (position.Y > image.ActualHeight) position.Y = image.ActualHeight;

            _startPoint = position;

            _selectionRectangle = new Rectangle
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2.5,
                StrokeDashArray = new DoubleCollection { 2 }
            };

            Canvas.SetLeft(_selectionRectangle, _startPoint.X);
            Canvas.SetTop(_selectionRectangle, _startPoint.Y);

            SelectionCanvas.Children.Add(_selectionRectangle);

            // 捕获鼠标事件
            Mouse.Capture(SelectionCanvas);
        }
    }


    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(image);
        MousePositionText.Text = $"[ {(int)(position.X / _scaleRatio)}, {(int)(position.Y / _scaleRatio)} ]";

        if (_selectionRectangle == null || e.LeftButton != MouseButtonState.Pressed)
            return;

        var pos = e.GetPosition(SelectionCanvas);

        var x = Math.Min(pos.X, _startPoint.X);
        var y = Math.Min(pos.Y, _startPoint.Y);

        var w = Math.Abs(pos.X - _startPoint.X);
        var h = Math.Abs(pos.Y - _startPoint.Y);

        // 确保矩形不会超出左边和上边
        if (x < 0)
        {
            x = 0;
            w = _startPoint.X;
        }

        if (y < 0)
        {
            y = 0;
            h = _startPoint.Y;
        }

        // 确保矩形不会超出右边和下边
        if (x + w > SelectionCanvas.ActualWidth)
        {
            w = SelectionCanvas.ActualWidth - x;
        }

        if (y + h > SelectionCanvas.ActualHeight)
        {
            h = SelectionCanvas.ActualHeight - y;
        }

        _selectionRectangle.Width = w;
        _selectionRectangle.Height = h;

        Canvas.SetLeft(_selectionRectangle, x);
        Canvas.SetTop(_selectionRectangle, y);

        MousePositionText.Text =
            $"[ {(int)(x / _scaleRatio)}, {(int)(y / _scaleRatio)}, {(int)(w / _scaleRatio)}, {(int)(h / _scaleRatio)} ]";
    }


    private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_selectionRectangle == null)
            return;

        // 释放鼠标捕获
        Mouse.Capture(null);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectionRectangle == null)
        {
            Growls.WarningGlobal("请选择一个区域");
            return;
        }

        var x = Canvas.GetLeft(_selectionRectangle) / _scaleRatio;
        var y = Canvas.GetTop(_selectionRectangle) / _scaleRatio;
        var w = _selectionRectangle.Width / _scaleRatio;
        var h = _selectionRectangle.Height / _scaleRatio;

        // 显示 x, y, w, h 值，可以替换为其他处理逻辑
        // Growl.InfoGlobal($"Roi: {x}, {y}, {w}, {h}");
        Output = new() { (int)x, (int)y, (int)w, (int)h };
        DialogResult = true;
        IsRoi = SelectType.SelectedIndex == 0;
        Close();
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        Close();
    }
}