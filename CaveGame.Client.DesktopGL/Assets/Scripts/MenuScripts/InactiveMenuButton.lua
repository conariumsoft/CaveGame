return function(text, parent)
    return TextButton(script, {
        Parent = parent,
        Text = text,
        TextColor = Color.White,
        Size = UICoords(0, -10, 1, 0.125),
        Font = GraphicsEngine.Instance.Fonts.Arial14,
        TextWrap = false,
        TextXAlign = TextXAlignment.Center,
        TextYAlign = TextYAlignment.Center,
        UnselectedBGColor = Color(0.05, 0.05, 0.05),
        SelectedBGColor = Color(0.05, 0.05, 0.05),
    })
end