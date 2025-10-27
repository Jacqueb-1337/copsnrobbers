.class public final La/a/a/e/a/KC_a;
.super Ljava/lang/Object;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        La/a/a/e/a/KC_a$KC_e;,
        La/a/a/e/a/KC_a$KC_d;,
        La/a/a/e/a/KC_a$KC_c;,
        La/a/a/e/a/KC_a$KC_a;,
        La/a/a/e/a/KC_a$KC_b;
    }
.end annotation


# static fields
.field private static final a:La/a/a/e/a/KC_a$KC_b;


# instance fields
.field private final b:Ljava/lang/Object;


# direct methods
.method static constructor <clinit>()V
    .locals 2

    .prologue
    .line 756
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x13

    if-lt v0, v1, :cond_0

    .line 757
    new-instance v0, La/a/a/e/a/KC_a$KC_e;

    invoke-direct {v0}, La/a/a/e/a/KC_a$KC_e;-><init>()V

    sput-object v0, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    .line 767
    :goto_0
    return-void

    .line 758
    :cond_0
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x12

    if-lt v0, v1, :cond_1

    .line 759
    new-instance v0, La/a/a/e/a/KC_a$KC_d;

    invoke-direct {v0}, La/a/a/e/a/KC_a$KC_d;-><init>()V

    sput-object v0, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    goto :goto_0

    .line 760
    :cond_1
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x10

    if-lt v0, v1, :cond_2

    .line 761
    new-instance v0, La/a/a/e/a/KC_a$KC_c;

    invoke-direct {v0}, La/a/a/e/a/KC_a$KC_c;-><init>()V

    sput-object v0, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    goto :goto_0

    .line 762
    :cond_2
    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0xe

    if-lt v0, v1, :cond_3

    .line 763
    new-instance v0, La/a/a/e/a/KC_a$KC_a;

    invoke-direct {v0}, La/a/a/e/a/KC_a$KC_a;-><init>()V

    sput-object v0, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    goto :goto_0

    .line 765
    :cond_3
    new-instance v0, La/a/a/e/a/KC_a$KC_b;

    invoke-direct {v0}, La/a/a/e/a/KC_a$KC_b;-><init>()V

    sput-object v0, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    goto :goto_0
.end method

.method public constructor <init>(Ljava/lang/Object;)V
    .locals 0

    .prologue
    .line 1080
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    .line 1081
    iput-object p1, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    .line 1082
    return-void
.end method


# virtual methods
.method public final a()Ljava/lang/Object;
    .locals 1

    .prologue
    .line 1088
    iget-object v0, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    return-object v0
.end method

.method public final a(I)V
    .locals 2

    .prologue
    .line 1295
    sget-object v0, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v1, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p1}, La/a/a/e/a/KC_a$KC_b;->a(Ljava/lang/Object;I)V

    .line 1296
    return-void
.end method

.method public final a(Ljava/lang/CharSequence;)V
    .locals 2

    .prologue
    .line 1815
    sget-object v0, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v1, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p1}, La/a/a/e/a/KC_a$KC_b;->a(Ljava/lang/Object;Ljava/lang/CharSequence;)V

    .line 1816
    return-void
.end method

.method public final a(Z)V
    .locals 2

    .prologue
    .line 1767
    sget-object v0, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v1, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1, p1}, La/a/a/e/a/KC_a$KC_b;->a(Ljava/lang/Object;Z)V

    .line 1768
    return-void
.end method

.method public final equals(Ljava/lang/Object;)Z
    .locals 4
    .param p1, "obj"    # Ljava/lang/Object;

    .prologue
    const/4 v0, 0x1

    const/4 v1, 0x0

    .line 1953
    if-ne p0, p1, :cond_1

    .line 1970
    .end local p1    # "obj":Ljava/lang/Object;
    :cond_0
    :goto_0
    return v0

    .line 1956
    .restart local p1    # "obj":Ljava/lang/Object;
    :cond_1
    if-nez p1, :cond_2

    move v0, v1

    .line 1957
    goto :goto_0

    .line 1959
    :cond_2
    invoke-virtual {p0}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v2

    invoke-virtual {p1}, Ljava/lang/Object;->getClass()Ljava/lang/Class;

    move-result-object v3

    if-eq v2, v3, :cond_3

    move v0, v1

    .line 1960
    goto :goto_0

    .line 1962
    :cond_3
    check-cast p1, La/a/a/e/a/KC_a;

    .line 1963
    .end local p1    # "obj":Ljava/lang/Object;
    iget-object v2, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    if-nez v2, :cond_4

    .line 1964
    iget-object v2, p1, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    if-eqz v2, :cond_0

    move v0, v1

    .line 1965
    goto :goto_0

    .line 1967
    :cond_4
    iget-object v2, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    iget-object v3, p1, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v2, v3}, Ljava/lang/Object;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-nez v2, :cond_0

    move v0, v1

    .line 1968
    goto :goto_0
.end method

.method public final hashCode()I
    .locals 1

    .prologue
    .line 1948
    iget-object v0, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    if-nez v0, :cond_0

    const/4 v0, 0x0

    :goto_0
    return v0

    :cond_0
    iget-object v0, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0}, Ljava/lang/Object;->hashCode()I

    move-result v0

    goto :goto_0
.end method

.method public final toString()Ljava/lang/String;
    .locals 4

    .prologue
    .line 1975
    new-instance v2, Ljava/lang/StringBuilder;

    invoke-direct {v2}, Ljava/lang/StringBuilder;-><init>()V

    .line 1976
    invoke-super {p0}, Ljava/lang/Object;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 1978
    new-instance v0, Landroid/graphics/Rect;

    invoke-direct {v0}, Landroid/graphics/Rect;-><init>()V

    .line 1980
    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3, v0}, La/a/a/e/a/KC_a$KC_b;->a(Ljava/lang/Object;Landroid/graphics/Rect;)V

    .line 1981
    new-instance v1, Ljava/lang/StringBuilder;

    const-string v3, "; boundsInParent: "

    invoke-direct {v1, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v2, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 1983
    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3, v0}, La/a/a/e/a/KC_a$KC_b;->b(Ljava/lang/Object;Landroid/graphics/Rect;)V

    .line 1984
    new-instance v1, Ljava/lang/StringBuilder;

    const-string v3, "; boundsInScreen: "

    invoke-direct {v1, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 1986
    const-string v0, "; packageName: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->d(Ljava/lang/Object;)Ljava/lang/CharSequence;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/CharSequence;)Ljava/lang/StringBuilder;

    .line 1987
    const-string v0, "; className: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->b(Ljava/lang/Object;)Ljava/lang/CharSequence;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/CharSequence;)Ljava/lang/StringBuilder;

    .line 1988
    const-string v0, "; text: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->e(Ljava/lang/Object;)Ljava/lang/CharSequence;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/CharSequence;)Ljava/lang/StringBuilder;

    .line 1989
    const-string v0, "; contentDescription: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->c(Ljava/lang/Object;)Ljava/lang/CharSequence;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/CharSequence;)Ljava/lang/StringBuilder;

    .line 1990
    const-string v0, "; viewId: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->p(Ljava/lang/Object;)Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 1992
    const-string v0, "; checkable: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->f(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    .line 1993
    const-string v0, "; checked: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->g(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    .line 1994
    const-string v0, "; focusable: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->j(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    .line 1995
    const-string v0, "; focused: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->k(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    .line 1996
    const-string v0, "; selected: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->o(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    .line 1997
    const-string v0, "; clickable: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->h(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    .line 1998
    const-string v0, "; longClickable: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->l(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    .line 1999
    const-string v0, "; enabled: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->i(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    .line 2000
    const-string v0, "; password: "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->m(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    .line 2001
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "; scrollable: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget-object v1, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v3, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v1, v3}, La/a/a/e/a/KC_a$KC_b;->n(Ljava/lang/Object;)Z

    move-result v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 2003
    const-string v0, "; ["

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 2004
    sget-object v0, La/a/a/e/a/KC_a;->a:La/a/a/e/a/KC_a$KC_b;

    iget-object v1, p0, La/a/a/e/a/KC_a;->b:Ljava/lang/Object;

    invoke-virtual {v0, v1}, La/a/a/e/a/KC_a$KC_b;->a(Ljava/lang/Object;)I

    move-result v0

    :goto_0
    if-eqz v0, :cond_1

    .line 2005
    const/4 v1, 0x1

    invoke-static {v0}, Ljava/lang/Integer;->numberOfTrailingZeros(I)I

    move-result v3

    shl-int v3, v1, v3

    .line 2006
    xor-int/lit8 v1, v3, -0x1

    and-int/2addr v1, v0

    .line 2007
    sparse-switch v3, :sswitch_data_0

    const-string v0, "ACTION_UNKNOWN"

    :goto_1
    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 2008
    if-eqz v1, :cond_0

    .line 2009
    const-string v0, ", "

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    :cond_0
    move v0, v1

    .line 2011
    goto :goto_0

    .line 2007
    :sswitch_0
    const-string v0, "ACTION_FOCUS"

    goto :goto_1

    :sswitch_1
    const-string v0, "ACTION_CLEAR_FOCUS"

    goto :goto_1

    :sswitch_2
    const-string v0, "ACTION_SELECT"

    goto :goto_1

    :sswitch_3
    const-string v0, "ACTION_CLEAR_SELECTION"

    goto :goto_1

    :sswitch_4
    const-string v0, "ACTION_CLICK"

    goto :goto_1

    :sswitch_5
    const-string v0, "ACTION_LONG_CLICK"

    goto :goto_1

    :sswitch_6
    const-string v0, "ACTION_ACCESSIBILITY_FOCUS"

    goto :goto_1

    :sswitch_7
    const-string v0, "ACTION_CLEAR_ACCESSIBILITY_FOCUS"

    goto :goto_1

    :sswitch_8
    const-string v0, "ACTION_NEXT_AT_MOVEMENT_GRANULARITY"

    goto :goto_1

    :sswitch_9
    const-string v0, "ACTION_PREVIOUS_AT_MOVEMENT_GRANULARITY"

    goto :goto_1

    :sswitch_a
    const-string v0, "ACTION_NEXT_HTML_ELEMENT"

    goto :goto_1

    :sswitch_b
    const-string v0, "ACTION_PREVIOUS_HTML_ELEMENT"

    goto :goto_1

    :sswitch_c
    const-string v0, "ACTION_SCROLL_FORWARD"

    goto :goto_1

    :sswitch_d
    const-string v0, "ACTION_SCROLL_BACKWARD"

    goto :goto_1

    :sswitch_e
    const-string v0, "ACTION_CUT"

    goto :goto_1

    :sswitch_f
    const-string v0, "ACTION_COPY"

    goto :goto_1

    :sswitch_10
    const-string v0, "ACTION_PASTE"

    goto :goto_1

    :sswitch_11
    const-string v0, "ACTION_SET_SELECTION"

    goto :goto_1

    .line 2012
    :cond_1
    const-string v0, "]"

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    .line 2014
    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    return-object v0

    .line 2007
    nop

    :sswitch_data_0
    .sparse-switch
        0x1 -> :sswitch_0
        0x2 -> :sswitch_1
        0x4 -> :sswitch_2
        0x8 -> :sswitch_3
        0x10 -> :sswitch_4
        0x20 -> :sswitch_5
        0x40 -> :sswitch_6
        0x80 -> :sswitch_7
        0x100 -> :sswitch_8
        0x200 -> :sswitch_9
        0x400 -> :sswitch_a
        0x800 -> :sswitch_b
        0x1000 -> :sswitch_c
        0x2000 -> :sswitch_d
        0x4000 -> :sswitch_f
        0x8000 -> :sswitch_10
        0x10000 -> :sswitch_e
        0x20000 -> :sswitch_11
    .end sparse-switch
.end method
