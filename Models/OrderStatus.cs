﻿using System;
using System.Collections.Generic;

namespace PizzaShop.Models;

public partial class OrderStatus
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;
}
