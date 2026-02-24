.class Lcom/kamcord/android/a/KC_f;
.super Lcom/kamcord/android/a/KC_a;


# instance fields
.field private a:Z

.field private b:Z

.field private c:Ljava/lang/String;


# direct methods
.method constructor <init>()V
    .locals 0

    invoke-direct {p0}, Lcom/kamcord/android/a/KC_a;-><init>()V

    return-void
.end method

.method static a(Lcom/kamcord/android/core/KC_f;Ljava/util/List;)V
    .locals 7
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Lcom/kamcord/android/core/KC_f;",
            "Ljava/util/List",
            "<",
            "Lcom/kamcord/android/core/KC_f;",
            ">;)V"
        }
    .end annotation

    const/4 v2, 0x1

    const/4 v3, 0x0

    iget v0, p0, Lcom/kamcord/android/core/KC_f;->b:I

    iget v1, p0, Lcom/kamcord/android/core/KC_f;->a:I

    if-le v0, v1, :cond_1

    move v1, v2

    :goto_0
    invoke-interface {p1}, Ljava/util/List;->iterator()Ljava/util/Iterator;

    move-result-object v5

    :cond_0
    :goto_1
    invoke-interface {v5}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_3

    invoke-interface {v5}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/core/KC_f;

    iget v4, v0, Lcom/kamcord/android/core/KC_f;->b:I

    iget v6, v0, Lcom/kamcord/android/core/KC_f;->a:I

    if-le v4, v6, :cond_2

    move v4, v2

    :goto_2
    if-eq v4, v1, :cond_0

    iget v4, v0, Lcom/kamcord/android/core/KC_f;->b:I

    iget v6, v0, Lcom/kamcord/android/core/KC_f;->a:I

    iput v6, v0, Lcom/kamcord/android/core/KC_f;->b:I

    iput v4, v0, Lcom/kamcord/android/core/KC_f;->a:I

    goto :goto_1

    :cond_1
    move v1, v3

    goto :goto_0

    :cond_2
    move v4, v3

    goto :goto_2

    :cond_3
    return-void
.end method

.method private a(Ljava/lang/String;)V
    .locals 1

    iput-object p1, p0, Lcom/kamcord/android/a/KC_f;->c:Ljava/lang/String;

    const/4 v0, 0x0

    iput-boolean v0, p0, Lcom/kamcord/android/a/KC_f;->b:Z

    return-void
.end method

.method private static a(Ljava/util/List;)V
    .locals 3
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Ljava/util/List",
            "<",
            "Lcom/kamcord/android/core/KC_f;",
            ">;)V"
        }
    .end annotation

    invoke-interface {p0}, Ljava/util/List;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/core/KC_f;

    iget v2, v0, Lcom/kamcord/android/core/KC_f;->a:I

    and-int/lit8 v2, v2, -0x10

    iput v2, v0, Lcom/kamcord/android/core/KC_f;->a:I

    iget v2, v0, Lcom/kamcord/android/core/KC_f;->b:I

    and-int/lit8 v2, v2, -0x10

    iput v2, v0, Lcom/kamcord/android/core/KC_f;->b:I

    goto :goto_0

    :cond_0
    return-void
.end method

.method private static p()Z
    .locals 2

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x10

    if-lt v0, v1, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method


# virtual methods
.method public a(Z)I
    .locals 1

    const/4 v0, 0x0

    return v0
.end method

.method public a(ZI)I
    .locals 0

    return p2
.end method

.method public a(Lcom/kamcord/android/core/KC_f;)Ljava/util/List;
    .locals 13
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "(",
            "Lcom/kamcord/android/core/KC_f;",
            ")",
            "Ljava/util/List",
            "<",
            "Lcom/kamcord/android/core/KC_f;",
            ">;"
        }
    .end annotation

    const/16 v12, 0x500

    const/4 v11, 0x5

    const/4 v10, 0x4

    const/4 v9, 0x0

    const/4 v8, 0x1

    new-instance v2, Ljava/util/ArrayList;

    invoke-direct {v2}, Ljava/util/ArrayList;-><init>()V

    invoke-virtual {p0}, Lcom/kamcord/android/a/KC_f;->c()Z

    move-result v0

    if-eqz v0, :cond_2

    if-nez p1, :cond_0

    move-object v0, v2

    :goto_0
    return-object v0

    :cond_0
    new-instance v0, Lcom/kamcord/android/core/KC_f;

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->a:I

    iget v3, p1, Lcom/kamcord/android/core/KC_f;->b:I

    invoke-direct {v0, v1, v3}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v0}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    new-instance v0, Lcom/kamcord/android/core/KC_f;

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->a:I

    div-int/lit8 v1, v1, 0x2

    iget v3, p1, Lcom/kamcord/android/core/KC_f;->b:I

    div-int/lit8 v3, v3, 0x2

    invoke-direct {v0, v1, v3}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v0}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    :cond_1
    :goto_1
    invoke-static {p1, v2}, Lcom/kamcord/android/a/KC_f;->a(Lcom/kamcord/android/core/KC_f;Ljava/util/List;)V

    invoke-static {v2}, Lcom/kamcord/android/a/KC_f;->a(Ljava/util/List;)V

    move-object v0, v2

    goto :goto_0

    :cond_2
    if-eqz p1, :cond_3

    iget v0, p1, Lcom/kamcord/android/core/KC_f;->a:I

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->b:I

    if-le v0, v1, :cond_6

    iget v0, p1, Lcom/kamcord/android/core/KC_f;->b:I

    :goto_2
    iget v1, p1, Lcom/kamcord/android/core/KC_f;->a:I

    iget v3, p1, Lcom/kamcord/android/core/KC_f;->b:I

    if-le v1, v3, :cond_7

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->a:I

    :goto_3
    int-to-float v1, v1

    int-to-float v0, v0

    div-float v0, v1, v0

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->a:I

    iget v3, p1, Lcom/kamcord/android/core/KC_f;->b:I

    mul-int/2addr v1, v3

    const v3, 0xd1f60

    if-le v1, v3, :cond_3

    float-to-double v4, v0

    const-wide/high16 v6, 0x3ff8000000000000L    # 1.5

    cmpl-double v1, v4, v6

    if-lez v1, :cond_3

    float-to-double v0, v0

    const-wide v4, 0x3ffd47ae147ae148L    # 1.83

    cmpg-double v0, v0, v4

    if-gez v0, :cond_3

    new-instance v0, Lcom/kamcord/android/core/KC_f;

    const/16 v1, 0x300

    invoke-direct {v0, v1, v12}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v0}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    :cond_3
    invoke-static {v8}, Landroid/media/CamcorderProfile;->get(I)Landroid/media/CamcorderProfile;

    move-result-object v0

    if-nez v0, :cond_8

    if-eqz p1, :cond_4

    new-instance v0, Lcom/kamcord/android/core/KC_f;

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->a:I

    iget v3, p1, Lcom/kamcord/android/core/KC_f;->b:I

    invoke-direct {v0, v1, v3}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v0}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    :cond_4
    new-instance v0, Lcom/kamcord/android/core/KC_f;

    const/16 v1, 0x2d0

    invoke-direct {v0, v1, v12}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v0}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    if-eqz p1, :cond_5

    new-instance v0, Lcom/kamcord/android/core/KC_f;

    iget v1, p1, Lcom/kamcord/android/core/KC_f;->a:I

    div-int/lit8 v1, v1, 0x2

    iget v3, p1, Lcom/kamcord/android/core/KC_f;->b:I

    div-int/lit8 v3, v3, 0x2

    invoke-direct {v0, v1, v3}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v0}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    :cond_5
    new-instance v0, Lcom/kamcord/android/core/KC_f;

    const/16 v1, 0x1e0

    const/16 v3, 0x280

    invoke-direct {v0, v1, v3}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v0}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    goto :goto_1

    :cond_6
    iget v0, p1, Lcom/kamcord/android/core/KC_f;->a:I

    goto :goto_2

    :cond_7
    iget v1, p1, Lcom/kamcord/android/core/KC_f;->b:I

    goto :goto_3

    :cond_8
    invoke-static {v11}, Landroid/media/CamcorderProfile;->hasProfile(I)Z

    move-result v0

    if-eqz v0, :cond_9

    invoke-static {v11}, Landroid/media/CamcorderProfile;->get(I)Landroid/media/CamcorderProfile;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    iget v3, v0, Landroid/media/CamcorderProfile;->videoFrameWidth:I

    iget v0, v0, Landroid/media/CamcorderProfile;->videoFrameHeight:I

    invoke-direct {v1, v3, v0}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v1}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    :cond_9
    invoke-static {v10}, Landroid/media/CamcorderProfile;->hasProfile(I)Z

    move-result v0

    if-eqz v0, :cond_a

    invoke-static {v10}, Landroid/media/CamcorderProfile;->get(I)Landroid/media/CamcorderProfile;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    iget v3, v0, Landroid/media/CamcorderProfile;->videoFrameWidth:I

    iget v0, v0, Landroid/media/CamcorderProfile;->videoFrameHeight:I

    invoke-direct {v1, v3, v0}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v1}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    :cond_a
    invoke-static {v8}, Landroid/media/CamcorderProfile;->hasProfile(I)Z

    move-result v0

    if-eqz v0, :cond_b

    invoke-static {v8}, Landroid/media/CamcorderProfile;->get(I)Landroid/media/CamcorderProfile;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    iget v3, v0, Landroid/media/CamcorderProfile;->videoFrameWidth:I

    iget v0, v0, Landroid/media/CamcorderProfile;->videoFrameHeight:I

    invoke-direct {v1, v3, v0}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v1}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    :cond_b
    invoke-static {v9}, Landroid/media/CamcorderProfile;->hasProfile(I)Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-static {v9}, Landroid/media/CamcorderProfile;->get(I)Landroid/media/CamcorderProfile;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    iget v3, v0, Landroid/media/CamcorderProfile;->videoFrameWidth:I

    iget v0, v0, Landroid/media/CamcorderProfile;->videoFrameHeight:I

    invoke-direct {v1, v3, v0}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-interface {v2, v1}, Ljava/util/List;->add(Ljava/lang/Object;)Z

    goto/16 :goto_1
.end method

.method public final a()Z
    .locals 5

    const/4 v4, 0x1

    iget-boolean v0, p0, Lcom/kamcord/android/a/KC_f;->a:Z

    if-nez v0, :cond_1

    iput-boolean v4, p0, Lcom/kamcord/android/a/KC_f;->b:Z

    invoke-static {}, Lcom/kamcord/android/a/KC_f;->p()Z

    move-result v0

    if-nez v0, :cond_0

    const-string v0, "Android SDK version 16 required."

    invoke-direct {p0, v0}, Lcom/kamcord/android/a/KC_f;->a(Ljava/lang/String;)V

    :cond_0
    sget-object v0, Landroid/os/Build;->BOARD:Ljava/lang/String;

    sget-object v1, Ljava/util/Locale;->ENGLISH:Ljava/util/Locale;

    invoke-virtual {v0, v1}, Ljava/lang/String;->toLowerCase(Ljava/util/Locale;)Ljava/lang/String;

    move-result-object v0

    sget-object v1, Landroid/os/Build;->DEVICE:Ljava/lang/String;

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->c()Z

    move-result v2

    if-eqz v2, :cond_3

    invoke-static {}, Lcom/kamcord/android/a/KC_f;->p()Z

    move-result v1

    if-nez v1, :cond_2

    const-string v0, "All boards whitelisted, Kamcord recording still disabled, because Android version incompatable."

    invoke-direct {p0, v0}, Lcom/kamcord/android/a/KC_f;->a(Ljava/lang/String;)V

    :goto_0
    iput-boolean v4, p0, Lcom/kamcord/android/a/KC_f;->a:Z

    :cond_1
    iget-boolean v0, p0, Lcom/kamcord/android/a/KC_f;->b:Z

    return v0

    :cond_2
    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "Testing on board: "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    goto :goto_0

    :cond_3
    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Device not whitelisted: board = "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v2, " device = "

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-direct {p0, v0}, Lcom/kamcord/android/a/KC_f;->a(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public b(Z)I
    .locals 1

    const/4 v0, 0x0

    return v0
.end method

.method public b(ZI)I
    .locals 0

    return p2
.end method

.method public final b()Z
    .locals 2

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0xe

    if-lt v0, v1, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public c(ZI)I
    .locals 1

    div-int/lit8 v0, p2, 0x2

    return v0
.end method

.method public c()Z
    .locals 2

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x12

    if-lt v0, v1, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public d(ZI)I
    .locals 1

    div-int/lit8 v0, p2, 0x2

    return v0
.end method

.method public final d()Z
    .locals 2

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x11

    if-lt v0, v1, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public e()Z
    .locals 1

    const/4 v0, 0x1

    return v0
.end method

.method public f()Z
    .locals 1

    const/4 v0, 0x1

    return v0
.end method

.method public g()Z
    .locals 1

    const/4 v0, 0x0

    return v0
.end method

.method public h()Z
    .locals 1

    const/4 v0, 0x1

    return v0
.end method

.method public i()Z
    .locals 1

    const/4 v0, 0x0

    return v0
.end method

.method public j()Ljava/lang/String;
    .locals 1

    const/4 v0, 0x0

    return-object v0
.end method

.method public k()Lcom/kamcord/android/core/KC_c;
    .locals 1

    const/4 v0, 0x0

    return-object v0
.end method

.method public final l()Ljava/lang/String;
    .locals 1

    iget-object v0, p0, Lcom/kamcord/android/a/KC_f;->c:Ljava/lang/String;

    return-object v0
.end method

.method public m()I
    .locals 1

    const/4 v0, 0x2

    return v0
.end method

.method public n()D
    .locals 2

    const-wide v0, 0x3f9b4e81b4f6c44eL    # 0.02666666667

    return-wide v0
.end method

.method public final o()Z
    .locals 2

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0x10

    if-lt v0, v1, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method
