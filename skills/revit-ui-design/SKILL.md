---
name: revit-ui-design
description: Design standards for Revit plugin WPF/XAML interfaces following the BuroMoscow (BM) brand. Includes color palette, typography and core UI styles. Use when designing new windows, dockable panes, or dialogs for Revit plugins.
---

# Revit UI Design Standards (BM Brand)

All BM plugins should share a consistent look and feel to provide a professional user experience. 

## 1. Core Color Palette

Use these brand colors for UI elements:

- **Primary Orange**: `#fc4614` (Used for buttons, accents, and progress)
- **Primary Orange Hover**: `#FF6F3D`
- **Text Dark**: `#333333` (Standard text color)
- **Text Light**: `#777777` (Secondary text, captions)
- **Border Light**: `#DDDDDD`
- **Background Light**: `#F5F5F5`

## 2. Resource Dictionary Template

Include these styles in your `Page.Resources` or `Window.Resources`:

```xml
<ResourceDictionary>
    <!-- Brushes -->
    <SolidColorBrush x:Key="PrimaryOrange" Color="#fc4614"/>
    <SolidColorBrush x:Key="TextDark" Color="#333333"/>
    <SolidColorBrush x:Key="TextLight" Color="#777777"/>
    <SolidColorBrush x:Key="BorderLight" Color="#DDDDDD"/>

    <!-- Typography -->
    <Style x:Key="HeaderStyle" TargetType="TextBlock">
        <Setter Property="FontSize" Value="18"/>
        <Setter Property="FontWeight" Value="Bold"/>
        <Setter Property="Foreground" Value="{StaticResource TextDark}"/>
    </Style>

    <!-- Buttons -->
    <Style x:Key="PrimaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource PrimaryOrange}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="Padding" Value="10,4"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border Background="{TemplateBinding Background}" CornerRadius="4" Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                    </Border>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>
</ResourceDictionary>
```

## 3. Typography Rules

- **Font Family**: Always use `Segoe UI`.
- **Main Heading**: 18pt, Bold.
- **Body Text**: 11pt - 12pt.
- **Secondary Text**: 10pt (Used for versions, footnotes).

## 4. UI Layout Principles

- **Paddings**: Maintain generous whitespace (15px to 20px for page margins).
- **Corner Radius**: Use `4` or `6` for buttons and containers to maintain a modern, softened look.
- **Icons**: Icons should be clean and follow the brand's minimalist aesthetic.

## 5. Revit Specifics
- **Dockable Panes**: Design with vertical scaling in mind (responsive layout).
- **Tooltips**: Always include descriptive tooltips for buttons.
