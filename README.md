# HCL color adjust

This is a Paint.net plugin that adjust the color an image using HCL/L\*a\*b\* color space.

## Usage

### Hue

This slider can be used to rotate the hue of all colors.

This is analogous to the "Hue" slider of the "Hue / Saturation" adjustment.

### Chroma (saturation)

There are 3 chroma slider: Add, Multiply, and Exponent.

Roughly speaking all 3 sliders modify the chroma in different ways. Which slider or combinations of sliders to use very much depends on the effect you are trying to achieve.

The Add and Multiply sliders are most similar to the "Saturation" slider of the "Hue / Saturation" adjustment.

### Luminance (lightness)

There are 3 luminance sliders: Add, Multiply, and Exponent.

The Multiply slider is the most useful and closely resembles the "Lightness" slider of the "Hue / Saturation" adjustment.

The Exponent slider can be used to adjust the contrast of the image. However, the effect differs from the "Brightness / Contrast" adjustment in that the chroma of the image is preserved.

The Add slider is mostly useful for some minor adjustments in combination with the other luminance sliders.

### a*, b*

These sliders add to the a\* and b\* components of L\*a\*b\* colors respectively. They adjust the magenta-green and cyan-red amounts of the image respectively.

## Development

This plugin is developed using [CodeLab](https://boltbait.com/pdn/CodeLab/).
