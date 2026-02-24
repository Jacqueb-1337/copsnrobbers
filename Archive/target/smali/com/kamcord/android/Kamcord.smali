.class public Lcom/kamcord/android/Kamcord;
.super Ljava/lang/Object;


# annotations
.annotation system Ldalvik/annotation/MemberClasses;
    value = {
        Lcom/kamcord/android/Kamcord$KC_a;,
        Lcom/kamcord/android/Kamcord$ShareTarget;,
        Lcom/kamcord/android/Kamcord$MetadataType;
    }
.end annotation


# static fields
.field public static final ANTI_SOCIAL:Z = true

.field public static final STANDARD:I = 0x0

.field public static final TRAILER:I = 0x1

.field private static U:I = 0x0

.field private static a:Lcom/kamcord/android/Kamcord; = null

.field public static final affinityInterval:J = 0x2540be400L

.field private static b:Lcom/kamcord/android/core/KC_u; = null

.field private static c:Ljava/lang/Object; = null

.field private static d:Lcom/kamcord/android/KC_o; = null

.field private static volatile e:Z = false

.field private static volatile f:Z = false

.field private static g:Lcom/kamcord/android/KC_V; = null

.field private static h:Z = false

.field private static i:Z = false

.field private static j:Z = false

.field private static k:Z = false

.field public static final kamcordBaseUrl:Ljava/lang/String; = "https://www.kamcord.com/"

.field private static l:Z = false

.field private static m:I = 0x0

.field private static n:Z = false

.field private static o:I = 0x0

.field private static p:Z = false

.field private static q:Z = false

.field private static r:J = 0x0L

.field private static u:Ljava/lang/String; = null

.field public static final version_:Ljava/lang/String; = "1.4.7"


# instance fields
.field private A:F

.field private B:I

.field private C:Ljava/lang/String;

.field private D:Ljava/lang/String;

.field private E:Ljava/lang/String;

.field private F:Ljava/lang/String;

.field private G:Ljava/lang/String;

.field private H:Ljava/lang/String;

.field private I:Ljava/lang/String;

.field private J:Ljava/lang/String;

.field private K:[I

.field private L:Z

.field private M:Ljava/lang/String;

.field private N:Ljava/lang/String;

.field private O:Landroid/app/Activity;

.field private P:Ljava/lang/String;

.field private Q:I

.field private R:Z

.field private S:Ljava/lang/String;

.field private T:Landroid/content/Context;

.field private s:Z

.field private t:Z

.field private v:Ljava/util/HashSet;
    .annotation system Ldalvik/annotation/Signature;
        value = {
            "Ljava/util/HashSet",
            "<",
            "Lcom/kamcord/android/KamcordListener;",
            ">;"
        }
    .end annotation
.end field

.field private w:Lcom/kamcord/android/KC_e;

.field private x:Lcom/kamcord/android/core/KC_H;

.field private y:Lcom/kamcord/android/core/KC_H;

.field private z:I


# direct methods
.method static constructor <clinit>()V
    .locals 9

    const/4 v8, 0x2

    const/4 v7, 0x1

    const/4 v6, 0x0

    const/4 v5, 0x0

    sput-object v6, Lcom/kamcord/android/Kamcord;->a:Lcom/kamcord/android/Kamcord;

    sput-object v6, Lcom/kamcord/android/Kamcord;->b:Lcom/kamcord/android/core/KC_u;

    sput-boolean v5, Lcom/kamcord/android/Kamcord;->e:Z

    sput-boolean v7, Lcom/kamcord/android/Kamcord;->f:Z

    sput-object v6, Lcom/kamcord/android/Kamcord;->g:Lcom/kamcord/android/KC_V;

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Device = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDevice()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Board = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getBoard()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Model = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget-object v1, Landroid/os/Build;->MODEL:Ljava/lang/String;

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Android SDK = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget v1, Landroid/os/Build$VERSION;->SDK_INT:I

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    sget-object v0, Lcom/kamcord/android/KC_o;->a:Lcom/kamcord/android/KC_o;

    sput-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    new-instance v0, Ljava/lang/Object;

    invoke-direct {v0}, Ljava/lang/Object;-><init>()V

    sput-object v0, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    :try_start_0
    const-string v0, "com.unity3d.player.UnityPlayer"

    invoke-static {v0}, Ljava/lang/Class;->forName(Ljava/lang/String;)Ljava/lang/Class;

    move-result-object v0

    const-string v1, "UnitySendMessage"

    const/4 v2, 0x3

    new-array v2, v2, [Ljava/lang/Class;

    const/4 v3, 0x0

    const-class v4, Ljava/lang/String;

    aput-object v4, v2, v3

    const/4 v3, 0x1

    const-class v4, Ljava/lang/String;

    aput-object v4, v2, v3

    const/4 v3, 0x2

    const-class v4, Ljava/lang/String;

    aput-object v4, v2, v3

    invoke-virtual {v0, v1, v2}, Ljava/lang/Class;->getMethod(Ljava/lang/String;[Ljava/lang/Class;)Ljava/lang/reflect/Method;

    move-result-object v1

    new-instance v2, Lcom/kamcord/android/KC_V;

    invoke-direct {v2, v0, v1}, Lcom/kamcord/android/KC_V;-><init>(Ljava/lang/Class;Ljava/lang/reflect/Method;)V

    sput-object v2, Lcom/kamcord/android/Kamcord;->g:Lcom/kamcord/android/KC_V;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->i()V
    :try_end_0
    .catch Ljava/lang/ClassNotFoundException; {:try_start_0 .. :try_end_0} :catch_0
    .catch Ljava/lang/NoSuchMethodException; {:try_start_0 .. :try_end_0} :catch_1

    :goto_0
    sput-boolean v5, Lcom/kamcord/android/Kamcord;->h:Z

    sput-boolean v5, Lcom/kamcord/android/Kamcord;->i:Z

    sput-boolean v5, Lcom/kamcord/android/Kamcord;->j:Z

    sput-boolean v7, Lcom/kamcord/android/Kamcord;->k:Z

    sput-boolean v5, Lcom/kamcord/android/Kamcord;->p:Z

    sput-boolean v5, Lcom/kamcord/android/Kamcord;->q:Z

    const-wide/16 v0, 0x0

    sput-wide v0, Lcom/kamcord/android/Kamcord;->r:J

    const-string v0, "kamcordPreferences"

    sput-object v0, Lcom/kamcord/android/Kamcord;->u:Ljava/lang/String;

    sput v8, Lcom/kamcord/android/Kamcord;->U:I

    return-void

    :catch_0
    move-exception v0

    sput-object v6, Lcom/kamcord/android/Kamcord;->g:Lcom/kamcord/android/KC_V;

    goto :goto_0

    :catch_1
    move-exception v0

    sput-object v6, Lcom/kamcord/android/Kamcord;->g:Lcom/kamcord/android/KC_V;

    goto :goto_0
.end method

.method private constructor <init>()V
    .locals 3

    const/4 v2, 0x0

    const/4 v1, 0x1

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    iput-boolean v1, p0, Lcom/kamcord/android/Kamcord;->s:Z

    iput-boolean v2, p0, Lcom/kamcord/android/Kamcord;->t:Z

    new-instance v0, Ljava/util/HashSet;

    invoke-direct {v0}, Ljava/util/HashSet;-><init>()V

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    const v0, 0x2625a0

    iput v0, p0, Lcom/kamcord/android/Kamcord;->z:I

    const/high16 v0, 0x41f00000    # 30.0f

    iput v0, p0, Lcom/kamcord/android/Kamcord;->A:F

    iput v1, p0, Lcom/kamcord/android/Kamcord;->B:I

    const/4 v0, 0x4

    new-array v0, v0, [I

    fill-array-data v0, :array_0

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->K:[I

    iput-boolean v1, p0, Lcom/kamcord/android/Kamcord;->L:Z

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->M:Ljava/lang/String;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->N:Ljava/lang/String;

    const/4 v0, 0x0

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->P:Ljava/lang/String;

    iput v2, p0, Lcom/kamcord/android/Kamcord;->Q:I

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->C:Ljava/lang/String;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->D:Ljava/lang/String;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->E:Ljava/lang/String;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->F:Ljava/lang/String;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->G:Ljava/lang/String;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->H:Ljava/lang/String;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->I:Ljava/lang/String;

    const-string v0, ""

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->J:Ljava/lang/String;

    return-void

    nop

    :array_0
    .array-data 4
        0x1
        0x0
        0x2
        0x3
    .end array-data
.end method

.method static a()I
    .locals 8

    const/16 v2, 0x9

    const/16 v3, 0x8

    const/4 v1, 0x0

    const/4 v0, 0x1

    :try_start_0
    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v4

    iget-object v4, v4, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-virtual {v4}, Landroid/app/Activity;->getRequestedOrientation()I

    move-result v4

    if-eq v4, v0, :cond_5

    if-eqz v4, :cond_5

    if-eq v4, v2, :cond_5

    if-eq v4, v3, :cond_5

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v4

    iget-object v4, v4, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-virtual {v4}, Landroid/app/Activity;->getWindowManager()Landroid/view/WindowManager;

    move-result-object v4

    invoke-interface {v4}, Landroid/view/WindowManager;->getDefaultDisplay()Landroid/view/Display;

    move-result-object v4

    invoke-virtual {v4}, Landroid/view/Display;->getRotation()I

    move-result v4

    new-instance v5, Landroid/util/DisplayMetrics;

    invoke-direct {v5}, Landroid/util/DisplayMetrics;-><init>()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v6

    iget-object v6, v6, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-virtual {v6}, Landroid/app/Activity;->getWindowManager()Landroid/view/WindowManager;

    move-result-object v6

    invoke-interface {v6}, Landroid/view/WindowManager;->getDefaultDisplay()Landroid/view/Display;

    move-result-object v6

    invoke-virtual {v6, v5}, Landroid/view/Display;->getMetrics(Landroid/util/DisplayMetrics;)V

    iget v6, v5, Landroid/util/DisplayMetrics;->widthPixels:I

    iget v5, v5, Landroid/util/DisplayMetrics;->heightPixels:I
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    if-eqz v4, :cond_0

    const/4 v7, 0x2

    if-ne v4, v7, :cond_1

    :cond_0
    if-gt v5, v6, :cond_3

    :cond_1
    if-eq v4, v0, :cond_2

    const/4 v7, 0x3

    if-ne v4, v7, :cond_4

    :cond_2
    if-le v6, v5, :cond_4

    :cond_3
    packed-switch v4, :pswitch_data_0

    const/4 v0, 0x7

    :goto_0
    :pswitch_0
    return v0

    :pswitch_1
    move v0, v1

    goto :goto_0

    :pswitch_2
    move v0, v2

    goto :goto_0

    :pswitch_3
    move v0, v3

    goto :goto_0

    :cond_4
    packed-switch v4, :pswitch_data_1

    const/4 v0, 0x6

    goto :goto_0

    :pswitch_4
    move v0, v1

    goto :goto_0

    :pswitch_5
    move v0, v2

    goto :goto_0

    :pswitch_6
    move v0, v3

    goto :goto_0

    :catch_0
    move-exception v0

    const-string v2, "Problem while determining activity orientation. Returning SCREEN_ORIENTATION_LANDSCAPE."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    move v0, v1

    goto :goto_0

    :cond_5
    move v0, v4

    goto :goto_0

    nop

    :pswitch_data_0
    .packed-switch 0x0
        :pswitch_0
        :pswitch_1
        :pswitch_2
        :pswitch_3
    .end packed-switch

    :pswitch_data_1
    .packed-switch 0x0
        :pswitch_4
        :pswitch_5
        :pswitch_6
        :pswitch_0
    .end packed-switch
.end method

.method static a(Ljava/lang/String;)I
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-direct {v0, p0}, Lcom/kamcord/android/Kamcord;->d(Ljava/lang/String;)I

    move-result v0

    return v0
.end method

.method private a(I)V
    .locals 1

    const/4 v0, 0x1

    if-ne p1, v0, :cond_0

    const v0, 0x4c4b40

    iput v0, p0, Lcom/kamcord/android/Kamcord;->z:I

    :goto_0
    return-void

    :cond_0
    const v0, 0x2625a0

    iput v0, p0, Lcom/kamcord/android/Kamcord;->z:I

    goto :goto_0
.end method

.method private a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;I)V
    .locals 0

    iput-object p1, p0, Lcom/kamcord/android/Kamcord;->M:Ljava/lang/String;

    iput-object p2, p0, Lcom/kamcord/android/Kamcord;->N:Ljava/lang/String;

    iput-object p3, p0, Lcom/kamcord/android/Kamcord;->P:Ljava/lang/String;

    iput p4, p0, Lcom/kamcord/android/Kamcord;->Q:I

    invoke-direct {p0}, Lcom/kamcord/android/Kamcord;->j()V

    return-void
.end method

.method public static addListener(Lcom/kamcord/android/KamcordListener;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0, p0}, Ljava/util/HashSet;->add(Ljava/lang/Object;)Z

    return-void
.end method

.method static b()Lcom/kamcord/android/core/KC_H;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->y:Lcom/kamcord/android/core/KC_H;

    return-object v0
.end method

.method private b(Ljava/lang/String;)Ljava/lang/String;
    .locals 4

    const-string v0, ""

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    const-string v2, "string"

    invoke-static {v2, p1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Landroid/content/Context;->getString(I)Ljava/lang/String;
    :try_end_0
    .catch Landroid/content/res/Resources$NotFoundException; {:try_start_0 .. :try_end_0} :catch_0

    move-result-object v0

    :goto_0
    return-object v0

    :catch_0
    move-exception v1

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "No string with name "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, "found!"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v1}, Landroid/content/res/Resources$NotFoundException;->printStackTrace()V

    goto :goto_0
.end method

.method public static bail()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->h()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->m()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->c()V

    return-void
.end method

.method public static beginDraw()V
    .locals 8

    const/4 v7, 0x1

    const/4 v6, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    sget-boolean v0, Lcom/kamcord/android/Kamcord;->p:Z

    if-eqz v0, :cond_2

    const-string v0, "beginDraw call with no complementary endDraw"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    goto :goto_0

    :cond_2
    sput-boolean v7, Lcom/kamcord/android/Kamcord;->p:Z

    sput-boolean v6, Lcom/kamcord/android/Kamcord;->q:Z

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isRecording()Z

    move-result v0

    if-eqz v0, :cond_6

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->n:Z

    if-nez v0, :cond_3

    sput v6, Lcom/kamcord/android/Kamcord;->o:I

    :cond_3
    sget v0, Lcom/kamcord/android/Kamcord;->o:I

    rem-int/lit16 v0, v0, 0xc8

    if-nez v0, :cond_4

    const-string v0, "beginDraw called..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    sput-boolean v7, Lcom/kamcord/android/Kamcord;->q:Z

    :cond_4
    sget v0, Lcom/kamcord/android/Kamcord;->o:I

    add-int/lit8 v0, v0, 0x1

    sput v0, Lcom/kamcord/android/Kamcord;->o:I

    sput-boolean v7, Lcom/kamcord/android/Kamcord;->n:Z

    :goto_1
    invoke-static {}, Ljava/lang/System;->nanoTime()J

    move-result-wide v0

    sget-wide v2, Lcom/kamcord/android/Kamcord;->r:J

    sub-long v2, v0, v2

    const-wide v4, 0x2540be400L

    cmp-long v2, v2, v4

    if-lez v2, :cond_5

    invoke-static {v6}, Lcom/kamcord/android/Kamcord;->setCurrentThreadAffinity(I)V

    sput-wide v0, Lcom/kamcord/android/Kamcord;->r:J

    :cond_5
    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->q()Z

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->q:Z

    if-eqz v0, :cond_0

    sput-boolean v6, Lcom/kamcord/android/Kamcord;->q:Z

    sget v0, Lcom/kamcord/android/Kamcord;->o:I

    rem-int/lit8 v0, v0, 0x64

    if-ne v0, v7, :cond_0

    const-string v0, "...done with beginDraw."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    goto :goto_0

    :cond_6
    sput-boolean v6, Lcom/kamcord/android/Kamcord;->n:Z

    goto :goto_1
.end method

.method public static blacklistAll()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    invoke-static {}, Lcom/kamcord/android/a/KC_c;->b()V

    goto :goto_0
.end method

.method public static blacklistAllBoards()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    const-string v0, "blacklistAllBoards() is deprecated.  Use blacklistAll() instead."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->b()V

    goto :goto_0
.end method

.method public static blacklistBoard(Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Blacklisting "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {p0}, Lcom/kamcord/android/a/KC_c;->b(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public static blacklistBoard(Ljava/lang/String;I)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Blacklisting "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " for sdk version "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {p0, p1}, Lcom/kamcord/android/a/KC_c;->b(Ljava/lang/String;I)V

    goto :goto_0
.end method

.method public static blacklistDevice(Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Blacklisting "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {p0}, Lcom/kamcord/android/a/KC_c;->d(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public static blacklistDevice(Ljava/lang/String;I)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Blacklisting "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " for sdk version "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {p0, p1}, Lcom/kamcord/android/a/KC_c;->d(Ljava/lang/String;I)V

    goto :goto_0
.end method

.method static synthetic c()I
    .locals 1

    sget v0, Lcom/kamcord/android/Kamcord;->U:I

    return v0
.end method

.method private c(Ljava/lang/String;)I
    .locals 4

    const/4 v0, 0x0

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    invoke-virtual {v1}, Landroid/content/Context;->getResources()Landroid/content/res/Resources;

    move-result-object v1

    const-string v2, "color"

    invoke-static {v2, p1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Landroid/content/res/Resources;->getColor(I)I
    :try_end_0
    .catch Landroid/content/res/Resources$NotFoundException; {:try_start_0 .. :try_end_0} :catch_0

    move-result v0

    :goto_0
    return v0

    :catch_0
    move-exception v1

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "No color with name "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, " found!"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v1}, Landroid/content/res/Resources$NotFoundException;->printStackTrace()V

    goto :goto_0
.end method

.method private d(Ljava/lang/String;)I
    .locals 4

    const/4 v0, 0x0

    :try_start_0
    iget-object v1, p0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    invoke-virtual {v1}, Landroid/content/Context;->getResources()Landroid/content/res/Resources;

    move-result-object v1

    const-string v2, "dimen"

    invoke-static {v2, p1}, Lcom/kamcord/android/Kamcord;->getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I

    move-result v2

    invoke-virtual {v1, v2}, Landroid/content/res/Resources;->getDimensionPixelSize(I)I
    :try_end_0
    .catch Landroid/content/res/Resources$NotFoundException; {:try_start_0 .. :try_end_0} :catch_0

    move-result v0

    :goto_0
    return v0

    :catch_0
    move-exception v1

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "No dimen with name "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    const-string v3, " found!"

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v1}, Landroid/content/res/Resources$NotFoundException;->printStackTrace()V

    goto :goto_0
.end method

.method private static d()Lcom/kamcord/android/Kamcord;
    .locals 1

    sget-object v0, Lcom/kamcord/android/Kamcord;->a:Lcom/kamcord/android/Kamcord;

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/Kamcord;

    invoke-direct {v0}, Lcom/kamcord/android/Kamcord;-><init>()V

    sput-object v0, Lcom/kamcord/android/Kamcord;->a:Lcom/kamcord/android/Kamcord;

    :cond_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->a:Lcom/kamcord/android/Kamcord;

    return-object v0
.end method

.method public static deleteFramebuffers()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->s()V

    :cond_0
    return-void
.end method

.method public static doneChangingWhitelist()V
    .locals 1

    const-string v0, "doneChangingWhitelist() is deprecated."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    const-string v0, "Perform all whitelist and blacklist modifications before initialization to enable/disable devices."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    return-void
.end method

.method private static e()Lcom/kamcord/android/core/KC_u;
    .locals 1

    sget-object v0, Lcom/kamcord/android/Kamcord;->b:Lcom/kamcord/android/core/KC_u;

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/core/KC_u;

    invoke-direct {v0}, Lcom/kamcord/android/core/KC_u;-><init>()V

    sput-object v0, Lcom/kamcord/android/Kamcord;->b:Lcom/kamcord/android/core/KC_u;

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->start()V

    :cond_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->b:Lcom/kamcord/android/core/KC_u;

    return-object v0
.end method

.method public static endDraw()V
    .locals 3

    const/4 v2, 0x1

    const/4 v1, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-nez v0, :cond_1

    :cond_0
    :goto_0
    return-void

    :cond_1
    sget-boolean v0, Lcom/kamcord/android/Kamcord;->p:Z

    if-eqz v0, :cond_0

    sput-boolean v1, Lcom/kamcord/android/Kamcord;->p:Z

    sput-boolean v1, Lcom/kamcord/android/Kamcord;->q:Z

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isRecording()Z

    move-result v0

    if-eqz v0, :cond_4

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->l:Z

    if-nez v0, :cond_2

    sput v1, Lcom/kamcord/android/Kamcord;->m:I

    :cond_2
    sget v0, Lcom/kamcord/android/Kamcord;->m:I

    rem-int/lit16 v0, v0, 0xc8

    if-nez v0, :cond_3

    const-string v0, "Recording, and endDraw getting called..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    sput-boolean v2, Lcom/kamcord/android/Kamcord;->q:Z

    :cond_3
    sget v0, Lcom/kamcord/android/Kamcord;->m:I

    add-int/lit8 v0, v0, 0x1

    sput v0, Lcom/kamcord/android/Kamcord;->m:I

    sput-boolean v2, Lcom/kamcord/android/Kamcord;->l:Z

    :goto_1
    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->r()V

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->q:Z

    if-eqz v0, :cond_0

    sput-boolean v1, Lcom/kamcord/android/Kamcord;->q:Z

    const-string v0, "...done with endDraw."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    goto :goto_0

    :cond_4
    sput-boolean v1, Lcom/kamcord/android/Kamcord;->l:Z

    goto :goto_1
.end method

.method private static f()V
    .locals 2

    sget-object v1, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/KC_o;->a:Lcom/kamcord/android/KC_o;

    sput-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    return-void

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method public static fatalError(Ljava/lang/String;)V
    .locals 3

    invoke-static {p0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    const-string v0, "Shutting down."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    sget-object v0, Lcom/kamcord/android/KC_o;->a:Lcom/kamcord/android/KC_o;

    sput-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->e:Z

    if-nez v0, :cond_0

    const/4 v0, 0x1

    sput-boolean v0, Lcom/kamcord/android/Kamcord;->e:Z

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordListener;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v2

    invoke-interface {v0, v2}, Lcom/kamcord/android/KamcordListener;->KamcordIsEnabledChanged(Z)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method public static fatalError()Z
    .locals 1

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->e:Z

    return v0
.end method

.method private static g()Lcom/kamcord/android/core/KC_H;
    .locals 2

    const/4 v0, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v1

    iget-object v1, v1, Lcom/kamcord/android/Kamcord;->x:Lcom/kamcord/android/core/KC_H;

    if-eqz v1, :cond_1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->x:Lcom/kamcord/android/core/KC_H;

    :cond_0
    :goto_0
    return-object v0

    :cond_1
    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v1

    iget-object v1, v1, Lcom/kamcord/android/Kamcord;->y:Lcom/kamcord/android/core/KC_H;

    if-eqz v1, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->y:Lcom/kamcord/android/core/KC_H;

    goto :goto_0
.end method

.method public static getActivity()Landroid/app/Activity;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    return-object v0
.end method

.method public static getApplicationContext()Landroid/content/Context;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    return-object v0
.end method

.method public static getApplicationName()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->P:Ljava/lang/String;

    :goto_0
    return-object v0

    :cond_0
    const-string v0, ""

    goto :goto_0
.end method

.method public static getAuthCenter()Lcom/kamcord/android/KC_e;
    .locals 3

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->b()Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v1, v0, Lcom/kamcord/android/Kamcord;->w:Lcom/kamcord/android/KC_e;

    if-nez v1, :cond_0

    new-instance v1, Lcom/kamcord/android/KC_e;

    iget-object v2, v0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-direct {v1, v2}, Lcom/kamcord/android/KC_e;-><init>(Landroid/content/Context;)V

    iput-object v1, v0, Lcom/kamcord/android/Kamcord;->w:Lcom/kamcord/android/KC_e;

    :cond_0
    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->w:Lcom/kamcord/android/KC_e;

    :goto_0
    return-object v0

    :cond_1
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static getAutomaticAudioRecording()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-boolean v0, v0, Lcom/kamcord/android/Kamcord;->s:Z

    return v0
.end method

.method public static getBoard()Ljava/lang/String;
    .locals 2

    sget-object v0, Landroid/os/Build;->BOARD:Ljava/lang/String;

    sget-object v1, Ljava/util/Locale;->ENGLISH:Ljava/util/Locale;

    invoke-virtual {v0, v1}, Ljava/lang/String;->toLowerCase(Ljava/util/Locale;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method public static getColor(Ljava/lang/String;)I
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-direct {v0, p0}, Lcom/kamcord/android/Kamcord;->c(Ljava/lang/String;)I

    move-result v0

    return v0
.end method

.method public static getDefaultEmailBody()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->I:Ljava/lang/String;

    :goto_0
    return-object v0

    :cond_0
    const-string v0, ""

    goto :goto_0
.end method

.method public static getDefaultEmailSubject()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->J:Ljava/lang/String;

    :goto_0
    return-object v0

    :cond_0
    const-string v0, ""

    goto :goto_0
.end method

.method public static getDefaultFacebookDescription()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->F:Ljava/lang/String;

    :goto_0
    return-object v0

    :cond_0
    const-string v0, ""

    goto :goto_0
.end method

.method public static getDefaultTweet()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->G:Ljava/lang/String;

    :goto_0
    return-object v0

    :cond_0
    const-string v0, ""

    goto :goto_0
.end method

.method public static getDefaultTwitterDescription()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->H:Ljava/lang/String;

    :goto_0
    return-object v0

    :cond_0
    const-string v0, ""

    goto :goto_0
.end method

.method public static getDefaultVideoTitle()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->C:Ljava/lang/String;

    :goto_0
    return-object v0

    :cond_0
    const-string v0, ""

    goto :goto_0
.end method

.method public static getDefaultYoutubeDescription()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->D:Ljava/lang/String;

    :goto_0
    return-object v0

    :cond_0
    const-string v0, ""

    goto :goto_0
.end method

.method public static getDefaultYoutubeKeywords()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->E:Ljava/lang/String;

    :goto_0
    return-object v0

    :cond_0
    const-string v0, ""

    goto :goto_0
.end method

.method public static getDelayAllocation()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-boolean v0, v0, Lcom/kamcord/android/Kamcord;->t:Z

    return v0
.end method

.method public static getDeveloperKey()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->M:Ljava/lang/String;

    return-object v0
.end method

.method public static getDeveloperSecret()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->N:Ljava/lang/String;

    return-object v0
.end method

.method public static getDevice()Ljava/lang/String;
    .locals 1

    sget-object v0, Landroid/os/Build;->DEVICE:Ljava/lang/String;

    return-object v0
.end method

.method public static getDisabledReason()Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->l()Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method public static getForegroundUploadService()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-boolean v0, v0, Lcom/kamcord/android/Kamcord;->L:Z

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static getResourceIdByName(Ljava/lang/String;Ljava/lang/String;)I
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v1, v0, Lcom/kamcord/android/Kamcord;->S:Ljava/lang/String;

    if-nez v1, :cond_0

    iget-object v1, v0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    invoke-virtual {v1}, Landroid/content/Context;->getPackageName()Ljava/lang/String;

    move-result-object v1

    iput-object v1, v0, Lcom/kamcord/android/Kamcord;->S:Ljava/lang/String;

    :cond_0
    iget-object v1, v0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    invoke-virtual {v1}, Landroid/content/Context;->getResources()Landroid/content/res/Resources;

    move-result-object v1

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->S:Ljava/lang/String;

    invoke-virtual {v1, p1, p0, v0}, Landroid/content/res/Resources;->getIdentifier(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)I

    move-result v0

    if-nez v0, :cond_1

    new-instance v1, Ljava/lang/StringBuilder;

    const-string v2, "No resource with name "

    invoke-direct {v1, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1, p1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, " found, there\'s probably a Resources.NotFoundException coming..."

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_1
    return v0
.end method

.method public static getScreenshot()Landroid/graphics/Bitmap;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->f()Landroid/graphics/Bitmap;

    move-result-object v0

    return-object v0
.end method

.method public static getShareTargets()[I
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->K:[I

    :goto_0
    return-object v0

    :cond_0
    const/4 v0, 0x0

    new-array v0, v0, [I

    goto :goto_0
.end method

.method public static getSharedPreferences()Landroid/content/SharedPreferences;
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    sget-object v1, Lcom/kamcord/android/Kamcord;->u:Ljava/lang/String;

    const/4 v2, 0x0

    invoke-virtual {v0, v1, v2}, Landroid/content/Context;->getSharedPreferences(Ljava/lang/String;I)Landroid/content/SharedPreferences;

    move-result-object v0

    :goto_0
    return-object v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static getString(Ljava/lang/String;)Ljava/lang/String;
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-direct {v0, p0}, Lcom/kamcord/android/Kamcord;->b(Ljava/lang/String;)Ljava/lang/String;

    move-result-object v0

    return-object v0
.end method

.method public static getVideoBitRate()I
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget v0, v0, Lcom/kamcord/android/Kamcord;->z:I

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static getVideoFrameInterval()I
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget v0, v0, Lcom/kamcord/android/Kamcord;->B:I

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static getVideoFrameRate()F
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget v0, v0, Lcom/kamcord/android/Kamcord;->A:F

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method private h()V
    .locals 3

    :try_start_0
    iget-object v0, p0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-virtual {v0}, Landroid/app/Activity;->getApplicationInfo()Landroid/content/pm/ApplicationInfo;

    move-result-object v0

    iget v0, v0, Landroid/content/pm/ApplicationInfo;->labelRes:I

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    iget-object v2, p0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-virtual {v2, v0}, Landroid/app/Activity;->getString(I)Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " Gameplay"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->C:Ljava/lang/String;
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v0, "An Awesome Gameplay"

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->C:Ljava/lang/String;

    goto :goto_0
.end method

.method private static i()V
    .locals 1

    sget-object v0, Lcom/kamcord/android/Kamcord;->g:Lcom/kamcord/android/KC_V;

    if-eqz v0, :cond_0

    sget-object v0, Lcom/kamcord/android/Kamcord;->g:Lcom/kamcord/android/KC_V;

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->addListener(Lcom/kamcord/android/KamcordListener;)V

    const/4 v0, 0x0

    sput-object v0, Lcom/kamcord/android/Kamcord;->g:Lcom/kamcord/android/KC_V;

    :cond_0
    return-void
.end method

.method public static initActivity(Landroid/app/Activity;)V
    .locals 1

    const-string v0, "initActivity called."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    if-nez p0, :cond_0

    const-string v0, "With null as argument."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    :cond_0
    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-direct {v0}, Lcom/kamcord/android/Kamcord;->j()V

    return-void
.end method

.method public static initKeyAndSecret(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Version = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->version()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const-string v0, "initKeyAndSecret called."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "key = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "appName = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    const/4 v1, 0x0

    invoke-direct {v0, p0, p1, p2, v1}, Lcom/kamcord/android/Kamcord;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;I)V

    return-void
.end method

.method public static initKeyAndSecret(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;I)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Version = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->version()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const-string v0, "initKeyAndSecret called with quality."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "key = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "appName = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "quality = "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-direct {v0, p0, p1, p2, p3}, Lcom/kamcord/android/Kamcord;->a(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;I)V

    return-void
.end method

.method public static isAntiSocial()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    const/4 v0, 0x1

    return v0
.end method

.method public static isEnabled()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-boolean v0, v0, Lcom/kamcord/android/Kamcord;->R:Z

    if-nez v0, :cond_0

    const-string v0, "Initialization not finished.  Must init activity, developer key, and developer secret."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_0
    sget-boolean v0, Lcom/kamcord/android/Kamcord;->e:Z

    if-eqz v0, :cond_1

    const/4 v0, 0x0

    :goto_0
    return v0

    :cond_1
    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->a()Z

    move-result v0

    goto :goto_0
.end method

.method public static isPaused()Z
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    sget-object v1, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-object v2, Lcom/kamcord/android/KC_o;->c:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_o;->equals(Ljava/lang/Object;)Z

    move-result v0

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :goto_0
    return v0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static isRecording()Z
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    sget-object v1, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-object v2, Lcom/kamcord/android/KC_o;->b:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_o;->equals(Ljava/lang/Object;)Z

    move-result v0

    monitor-exit v1
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    :goto_0
    return v0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static isWhitelisted()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->c()Z

    move-result v0

    return v0
.end method

.method public static isWhitelisted(Ljava/lang/String;)Z
    .locals 1

    const-string v0, "isWhitelisted(String) is deprecated.  Use isWhitelisted() with no arguments instead."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->c()Z

    move-result v0

    return v0
.end method

.method private j()V
    .locals 4

    const/4 v2, 0x0

    const/4 v1, 0x1

    iget-boolean v0, p0, Lcom/kamcord/android/Kamcord;->R:Z

    if-nez v0, :cond_3

    iget-object v0, p0, Lcom/kamcord/android/Kamcord;->M:Ljava/lang/String;

    const-string v3, ""

    invoke-virtual {v0, v3}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_4

    iget-object v0, p0, Lcom/kamcord/android/Kamcord;->N:Ljava/lang/String;

    const-string v3, ""

    invoke-virtual {v0, v3}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_4

    iget-object v0, p0, Lcom/kamcord/android/Kamcord;->P:Ljava/lang/String;

    const-string v3, ""

    invoke-virtual {v0, v3}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_4

    iget-object v0, p0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    if-eqz v0, :cond_4

    move v0, v1

    :goto_0
    if-eqz v0, :cond_3

    iput-boolean v1, p0, Lcom/kamcord/android/Kamcord;->R:Z

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->a()Z

    move-result v0

    if-eqz v0, :cond_0

    :try_start_0
    const-string v0, "Loading libkamcordcore.so..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const-string v0, "kamcordcore"

    invoke-static {v0}, Ljava/lang/System;->loadLibrary(Ljava/lang/String;)V

    const-string v0, "...done loading libkamcordcore.so."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_0
    .catch Ljava/lang/Throwable; {:try_start_0 .. :try_end_0} :catch_0

    :cond_0
    :goto_1
    :try_start_1
    iget-object v0, p0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-virtual {v0}, Landroid/app/Activity;->getApplicationContext()Landroid/content/Context;

    move-result-object v0

    iput-object v0, p0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    iget-object v1, p0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-virtual {v0, v1}, Lcom/kamcord/android/core/KC_u;->a(Landroid/app/Activity;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    invoke-static {}, Lcom/kamcord/android/core/KC_u;->e()V

    iget v0, p0, Lcom/kamcord/android/Kamcord;->Q:I

    invoke-direct {p0, v0}, Lcom/kamcord/android/Kamcord;->a(I)V

    sget v0, Landroid/os/Build$VERSION;->SDK_INT:I

    const/16 v1, 0xe

    if-lt v0, v1, :cond_1

    iget-object v0, p0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    invoke-static {}, Lcom/kamcord/android/KC_n;->a()Lcom/kamcord/android/KC_n;

    move-result-object v1

    invoke-virtual {v0, v1}, Landroid/app/Activity;->registerComponentCallbacks(Landroid/content/ComponentCallbacks;)V

    :cond_1
    new-instance v0, Ljava/lang/StringBuilder;

    invoke-direct {v0}, Ljava/lang/StringBuilder;-><init>()V

    invoke-static {}, Landroid/os/Environment;->getExternalStorageDirectory()Ljava/io/File;

    move-result-object v1

    invoke-virtual {v1}, Ljava/io/File;->getPath()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, "/Kamcord"

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    new-instance v1, Ljava/io/File;

    invoke-direct {v1, v0}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    invoke-virtual {v1}, Ljava/io/File;->mkdir()Z

    new-instance v1, Ljava/lang/StringBuilder;

    invoke-direct {v1}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v1, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    const-string v2, "/"

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    iget-object v2, p0, Lcom/kamcord/android/Kamcord;->M:Ljava/lang/String;

    invoke-virtual {v1, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    new-instance v2, Ljava/io/File;

    invoke-direct {v2, v1}, Ljava/io/File;-><init>(Ljava/lang/String;)V

    invoke-static {v2}, Lcom/kamcord/android/core/KC_S;->a(Ljava/io/File;)V

    invoke-virtual {v2}, Ljava/io/File;->mkdir()Z

    invoke-static {v1}, Lcom/kamcord/android/core/KC_S;->a(Ljava/lang/String;)V

    iget-object v1, p0, Lcom/kamcord/android/Kamcord;->O:Landroid/app/Activity;

    const-string v2, "proximanova_light"

    const-string v3, "otf"

    invoke-static {v1, v0, v2, v3}, Lcom/kamcord/android/KC_e;->a(Landroid/content/Context;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V

    invoke-direct {p0}, Lcom/kamcord/android/Kamcord;->h()V

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->j:Z

    if-eqz v0, :cond_2

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->i:Z

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->setVoiceOverlayActivated(Z)V
    :try_end_1
    .catch Ljava/lang/Throwable; {:try_start_1 .. :try_end_1} :catch_2

    :cond_2
    const/4 v0, 0x5

    :try_start_2
    invoke-static {v0}, Lcom/kamcord/android/KC_m;->a(I)V
    :try_end_2
    .catch Ljava/lang/Throwable; {:try_start_2 .. :try_end_2} :catch_1

    :cond_3
    :goto_2
    return-void

    :cond_4
    move v0, v2

    goto/16 :goto_0

    :catch_0
    move-exception v0

    sput-boolean v2, Lcom/kamcord/android/Kamcord;->f:Z

    const-string v2, "Unable to link libkamcordcore.so. Disabling Kamcord."

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    sput-boolean v1, Lcom/kamcord/android/Kamcord;->e:Z

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    goto/16 :goto_1

    :catch_1
    move-exception v0

    :try_start_3
    const-string v1, "Error tracking launch event!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V
    :try_end_3
    .catch Ljava/lang/Throwable; {:try_start_3 .. :try_end_3} :catch_2

    goto :goto_2

    :catch_2
    move-exception v0

    invoke-virtual {v0}, Ljava/lang/Throwable;->printStackTrace()V

    const-string v0, "Error while finishing initialization!"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->fatalError(Ljava/lang/String;)V

    goto :goto_2
.end method

.method private static k()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-boolean v0, v0, Lcom/kamcord/android/Kamcord;->R:Z

    if-eqz v0, :cond_0

    const-string v0, "Calling whitelisting or blacklisting functions after initialization is deprecated!"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    const-string v0, "Perform all whitelist and blacklist modifications before initialization to enable/disable devices."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_0
    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-boolean v0, v0, Lcom/kamcord/android/Kamcord;->R:Z

    return v0
.end method

.method public static notifyVideoFinishedUploading(Ljava/lang/String;Z)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordListener;

    invoke-interface {v0, p0, p1}, Lcom/kamcord/android/KamcordListener;->KamcordVideoFinishedUploading(Ljava/lang/String;Z)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method public static notifyVideoSharedTo(Ljava/lang/String;Ljava/lang/String;Z)V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_5

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v1

    if-eqz v1, :cond_3

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :cond_0
    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_3

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordListener;

    invoke-interface {v0, p0, p1, p2}, Lcom/kamcord/android/KamcordListener;->KamcordVideoSharedTo(Ljava/lang/String;Ljava/lang/String;Z)V

    const-string v2, "Facebook"

    invoke-virtual {p1, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_1

    invoke-interface {v0, p0, p2}, Lcom/kamcord/android/KamcordListener;->KamcordVideoSharedToFacebook(Ljava/lang/String;Z)V

    :cond_1
    const-string v2, "Twitter"

    invoke-virtual {p1, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_2

    invoke-interface {v0, p0, p2}, Lcom/kamcord/android/KamcordListener;->KamcordVideoSharedToTwitter(Ljava/lang/String;Z)V

    :cond_2
    const-string v2, "YouTube"

    invoke-virtual {p1, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_0

    invoke-interface {v0, p0, p2}, Lcom/kamcord/android/KamcordListener;->KamcordVideoSharedToYoutube(Ljava/lang/String;Z)V

    goto :goto_0

    :cond_3
    if-eqz p2, :cond_5

    const-string v0, "Facebook"

    invoke-virtual {p1, v0}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_4

    const/4 v0, 0x6

    invoke-static {v0}, Lcom/kamcord/android/KC_m;->a(I)V

    :cond_4
    const-string v0, "Twitter"

    invoke-virtual {p1, v0}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_5

    const/4 v0, 0x7

    invoke-static {v0}, Lcom/kamcord/android/KC_m;->a(I)V

    :cond_5
    return-void
.end method

.method public static notifyVideoThumbnailReadyAtFilePath(Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordListener;

    invoke-interface {v0, p0}, Lcom/kamcord/android/KamcordListener;->KamcordVideoThumbnailReadyAtFilePath(Ljava/lang/String;)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method public static notifyVideoUploadProgressed(Ljava/lang/String;F)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordListener;

    invoke-interface {v0, p0, p1}, Lcom/kamcord/android/KamcordListener;->KamcordVideoUploadProgressed(Ljava/lang/String;F)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method public static notifyVideoWillBeginUploading(Ljava/lang/String;Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordListener;

    invoke-interface {v0, p0, p1}, Lcom/kamcord/android/KamcordListener;->KamcordVideoWillBeginUploading(Ljava/lang/String;Ljava/lang/String;)V

    goto :goto_0

    :cond_0
    return-void
.end method

.method public static notifyViewDidAppear()V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordListener;

    invoke-interface {v0}, Lcom/kamcord/android/KamcordListener;->KamcordViewDidAppear()V

    goto :goto_0

    :cond_0
    return-void
.end method

.method public static notifyViewDidDisappear()V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->f()V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v1

    if-eqz v1, :cond_0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordListener;

    invoke-interface {v0}, Lcom/kamcord/android/KamcordListener;->KamcordViewDidDisappear()V

    goto :goto_0

    :cond_0
    return-void
.end method

.method public static notifyViewDidNotAppear(Z)V
    .locals 2

    if-eqz p0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->f()V

    :cond_0
    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v1

    if-eqz v1, :cond_1

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0}, Ljava/util/HashSet;->iterator()Ljava/util/Iterator;

    move-result-object v1

    :goto_0
    invoke-interface {v1}, Ljava/util/Iterator;->hasNext()Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-interface {v1}, Ljava/util/Iterator;->next()Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/KamcordListener;

    invoke-interface {v0}, Lcom/kamcord/android/KamcordListener;->KamcordViewDidNotAppear()V

    goto :goto_0

    :cond_1
    return-void
.end method

.method public static pauseRecording()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    sget-object v1, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-object v2, Lcom/kamcord/android/KC_o;->b:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_o;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->i()V

    sget-object v0, Lcom/kamcord/android/KC_o;->c:Lcom/kamcord/android/KC_o;

    sput-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    :goto_0
    monitor-exit v1

    :cond_0
    return-void

    :cond_1
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "pauseRecording called with state:"

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget-object v2, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method public static removeListener(Lcom/kamcord/android/KamcordListener;)Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->v:Ljava/util/HashSet;

    invoke-virtual {v0, p0}, Ljava/util/HashSet;->remove(Ljava/lang/Object;)Z

    move-result v0

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static resumeRecording()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    sget-object v1, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-object v2, Lcom/kamcord/android/KC_o;->c:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_o;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->j()V

    sget-object v0, Lcom/kamcord/android/KC_o;->b:Lcom/kamcord/android/KC_o;

    sput-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    :goto_0
    monitor-exit v1

    :cond_0
    return-void

    :cond_1
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "resumeRecording called with state:"

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget-object v2, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0
.end method

.method public static setApplicationContextFrom(Landroid/content/Context;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iget-object v0, v0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    if-nez v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-virtual {p0}, Landroid/content/Context;->getApplicationContext()Landroid/content/Context;

    move-result-object v1

    iput-object v1, v0, Lcom/kamcord/android/Kamcord;->T:Landroid/content/Context;

    :cond_0
    return-void
.end method

.method public static setAudioSettings(II)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0, p0, p1}, Lcom/kamcord/android/core/KC_u;->a(II)V

    :cond_0
    return-void
.end method

.method public static setAudioSource(Lcom/kamcord/android/AudioSource;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0, p0}, Lcom/kamcord/android/core/KC_u;->a(Lcom/kamcord/android/AudioSource;)V

    :cond_0
    return-void
.end method

.method public static setAutomaticAudioRecording(Z)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-boolean p0, v0, Lcom/kamcord/android/Kamcord;->s:Z

    iget-boolean v0, v0, Lcom/kamcord/android/Kamcord;->s:Z

    if-nez v0, :cond_0

    const/4 v0, 0x0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->setAudioSource(Lcom/kamcord/android/AudioSource;)V

    :cond_0
    return-void
.end method

.method public static setCurrentThreadAffinity(I)V
    .locals 1

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->f:Z

    if-eqz v0, :cond_0

    invoke-static {p0}, Lcom/kamcord/android/core/KamcordNative;->setCurrentThreadAffinity(I)V

    :cond_0
    return-void
.end method

.method public static setCurrentVideo(Lcom/kamcord/android/core/KC_H;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->x:Lcom/kamcord/android/core/KC_H;

    return-void
.end method

.method public static setDefaultEmailBody(Ljava/lang/String;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->I:Ljava/lang/String;

    :cond_0
    return-void
.end method

.method public static setDefaultEmailSubject(Ljava/lang/String;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->J:Ljava/lang/String;

    :cond_0
    return-void
.end method

.method public static setDefaultFacebookDescription(Ljava/lang/String;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->F:Ljava/lang/String;

    :cond_0
    return-void
.end method

.method public static setDefaultTweet(Ljava/lang/String;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->G:Ljava/lang/String;

    :cond_0
    return-void
.end method

.method public static setDefaultTwitterDescription(Ljava/lang/String;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->H:Ljava/lang/String;

    :cond_0
    return-void
.end method

.method public static setDefaultVideoTitle(Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    if-eqz p0, :cond_1

    invoke-virtual {p0}, Ljava/lang/String;->length()I

    move-result v1

    if-lez v1, :cond_1

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->C:Ljava/lang/String;

    :cond_0
    :goto_0
    return-void

    :cond_1
    const-string v1, "The default video title must be non-empty."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    iget-object v1, v0, Lcom/kamcord/android/Kamcord;->C:Ljava/lang/String;

    if-eqz v1, :cond_2

    iget-object v1, v0, Lcom/kamcord/android/Kamcord;->C:Ljava/lang/String;

    invoke-virtual {v1}, Ljava/lang/String;->length()I

    move-result v1

    if-nez v1, :cond_0

    :cond_2
    invoke-direct {v0}, Lcom/kamcord/android/Kamcord;->h()V

    goto :goto_0
.end method

.method public static setDefaultYoutubeDescription(Ljava/lang/String;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->D:Ljava/lang/String;

    :cond_0
    return-void
.end method

.method public static setDefaultYoutubeKeywords(Ljava/lang/String;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->E:Ljava/lang/String;

    :cond_0
    return-void
.end method

.method public static setDelayAllocation(Z)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-boolean p0, v0, Lcom/kamcord/android/Kamcord;->t:Z

    return-void
.end method

.method public static setDeveloperMetadata(ILjava/lang/String;Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->g()Lcom/kamcord/android/core/KC_H;

    move-result-object v0

    if-eqz v0, :cond_0

    :try_start_0
    invoke-virtual {v0, p0, p1, p2}, Lcom/kamcord/android/core/KC_H;->a(ILjava/lang/String;Ljava/lang/String;)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :cond_0
    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v1, "Couldn\'t set developer metadata!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    goto :goto_0
.end method

.method public static setDeveloperMetadataWithNumericValue(ILjava/lang/String;Ljava/lang/String;D)V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->g()Lcom/kamcord/android/core/KC_H;

    move-result-object v0

    if-eqz v0, :cond_0

    :try_start_0
    invoke-static {p3, p4}, Ljava/lang/Double;->valueOf(D)Ljava/lang/Double;

    move-result-object v1

    invoke-virtual {v0, p0, p1, p2, v1}, Lcom/kamcord/android/core/KC_H;->a(ILjava/lang/String;Ljava/lang/String;Ljava/lang/Double;)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :cond_0
    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v1, "Couldn\'t set developer metadata!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    goto :goto_0
.end method

.method public static setDeveloperMetadataWithNumericValue(ILjava/lang/String;Ljava/lang/String;F)V
    .locals 4

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->g()Lcom/kamcord/android/core/KC_H;

    move-result-object v0

    if-eqz v0, :cond_0

    float-to-double v2, p3

    :try_start_0
    invoke-static {v2, v3}, Ljava/lang/Double;->valueOf(D)Ljava/lang/Double;

    move-result-object v1

    invoke-virtual {v0, p0, p1, p2, v1}, Lcom/kamcord/android/core/KC_H;->a(ILjava/lang/String;Ljava/lang/String;Ljava/lang/Double;)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :cond_0
    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v1, "Couldn\'t set developer metadata!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    goto :goto_0
.end method

.method public static setDeveloperMetadataWithNumericValue(ILjava/lang/String;Ljava/lang/String;I)V
    .locals 4

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->g()Lcom/kamcord/android/core/KC_H;

    move-result-object v0

    if-eqz v0, :cond_0

    int-to-double v2, p3

    :try_start_0
    invoke-static {v2, v3}, Ljava/lang/Double;->valueOf(D)Ljava/lang/Double;

    move-result-object v1

    invoke-virtual {v0, p0, p1, p2, v1}, Lcom/kamcord/android/core/KC_H;->a(ILjava/lang/String;Ljava/lang/String;Ljava/lang/Double;)V
    :try_end_0
    .catch Lorg/json/JSONException; {:try_start_0 .. :try_end_0} :catch_0

    :cond_0
    :goto_0
    return-void

    :catch_0
    move-exception v0

    const-string v1, "Couldn\'t set developer metadata!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->d(Ljava/lang/String;)I

    invoke-virtual {v0}, Lorg/json/JSONException;->printStackTrace()V

    goto :goto_0
.end method

.method public static setForegroundUploadService(Z)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-boolean p0, v0, Lcom/kamcord/android/Kamcord;->L:Z

    :cond_0
    return-void
.end method

.method public static setLastVideo(Lcom/kamcord/android/core/KC_H;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    iput-object p0, v0, Lcom/kamcord/android/Kamcord;->y:Lcom/kamcord/android/core/KC_H;

    return-void
.end method

.method public static setLevel(Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "setLevel() is deprecated. Use setDeveloperMetadata() instead."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    const/4 v0, 0x0

    const-string v1, "Level"

    invoke-static {v0, v1, p0}, Lcom/kamcord/android/Kamcord;->setDeveloperMetadata(ILjava/lang/String;Ljava/lang/String;)V

    :cond_0
    return-void
.end method

.method public static setLevelAndScore(Ljava/lang/String;D)V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "setLevelAndScore() is deprecated. Use setDeveloperMetadata() instead."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    const/4 v0, 0x0

    const-string v1, "Level"

    invoke-static {v0, v1, p0}, Lcom/kamcord/android/Kamcord;->setDeveloperMetadata(ILjava/lang/String;Ljava/lang/String;)V

    const/4 v0, 0x1

    const-string v1, "Score"

    invoke-static {p1, p2}, Ljava/lang/Double;->toString(D)Ljava/lang/String;

    move-result-object v2

    invoke-static {v0, v1, v2, p1, p2}, Lcom/kamcord/android/Kamcord;->setDeveloperMetadataWithNumericValue(ILjava/lang/String;Ljava/lang/String;D)V

    :cond_0
    return-void
.end method

.method public static setLoggingEnabled(Z)V
    .locals 1

    if-eqz p0, :cond_0

    const/4 v0, 0x2

    sput v0, Lcom/kamcord/android/Kamcord;->U:I

    :goto_0
    return-void

    :cond_0
    const/16 v0, 0x8

    sput v0, Lcom/kamcord/android/Kamcord;->U:I

    goto :goto_0
.end method

.method public static setLoggingLevel(I)V
    .locals 0

    sput p0, Lcom/kamcord/android/Kamcord;->U:I

    return-void
.end method

.method public static setPerformanceTestEnabled(Z)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0, p0}, Lcom/kamcord/android/core/KC_u;->b(Z)V

    return-void
.end method

.method public static setScore(D)V
    .locals 4

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "setScore() is deprecated. Use setDeveloperMetadata() instead."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    const/4 v0, 0x1

    const-string v1, "Score"

    invoke-static {p0, p1}, Ljava/lang/Double;->toString(D)Ljava/lang/String;

    move-result-object v2

    invoke-static {v0, v1, v2, p0, p1}, Lcom/kamcord/android/Kamcord;->setDeveloperMetadataWithNumericValue(ILjava/lang/String;Ljava/lang/String;D)V

    :cond_0
    return-void
.end method

.method public static setShareTargets([I)V
    .locals 9

    const/4 v8, -0x1

    const/4 v7, 0x4

    const/4 v1, 0x1

    const/4 v2, 0x0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_7

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v5

    array-length v0, p0

    if-ne v0, v7, :cond_1

    move v0, v1

    :goto_0
    move v4, v2

    :goto_1
    if-ge v4, v7, :cond_6

    if-eqz v0, :cond_6

    if-eqz v0, :cond_3

    aget v0, p0, v4

    if-eq v0, v8, :cond_0

    if-eqz v0, :cond_0

    if-eq v0, v1, :cond_0

    const/4 v3, 0x2

    if-eq v0, v3, :cond_0

    const/4 v3, 0x3

    if-eq v0, v3, :cond_0

    const/4 v3, 0x5

    if-ne v0, v3, :cond_2

    :cond_0
    move v0, v1

    :goto_2
    if-eqz v0, :cond_3

    move v0, v1

    :goto_3
    add-int/lit8 v3, v4, 0x1

    :goto_4
    if-ge v3, v7, :cond_5

    if-eqz v0, :cond_5

    aget v6, p0, v4

    if-eq v6, v8, :cond_5

    if-eqz v0, :cond_4

    aget v0, p0, v4

    aget v6, p0, v3

    if-eq v0, v6, :cond_4

    move v0, v1

    :goto_5
    add-int/lit8 v3, v3, 0x1

    goto :goto_4

    :cond_1
    move v0, v2

    goto :goto_0

    :cond_2
    move v0, v2

    goto :goto_2

    :cond_3
    move v0, v2

    goto :goto_3

    :cond_4
    move v0, v2

    goto :goto_5

    :cond_5
    add-int/lit8 v3, v4, 0x1

    move v4, v3

    goto :goto_1

    :cond_6
    if-eqz v0, :cond_8

    array-length v0, p0

    invoke-static {p0, v0}, Ljava/util/Arrays;->copyOf([II)[I

    move-result-object v0

    iput-object v0, v5, Lcom/kamcord/android/Kamcord;->K:[I

    :cond_7
    :goto_6
    return-void

    :cond_8
    const-string v0, "Invalid arguments to setShareTargets()!"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    const-string v0, "You must specify exactly four unique share targets!"

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    goto :goto_6
.end method

.method public static setVideoIncompleteWarningEnabled(Z)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    sput-boolean p0, Lcom/kamcord/android/Kamcord;->k:Z

    :cond_0
    return-void
.end method

.method public static setVideoMetadata(Ljava/lang/String;)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "setVideoMetadata() is deprecated. Use setDeveloperMetadata() instead."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->c(Ljava/lang/String;)I

    :cond_0
    return-void
.end method

.method public static setVideoQuality(I)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    move-result-object v0

    invoke-direct {v0, p0}, Lcom/kamcord/android/Kamcord;->a(I)V

    :cond_0
    return-void
.end method

.method public static setVoiceOverlayActivated(Z)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    sput-boolean p0, Lcom/kamcord/android/Kamcord;->i:Z

    const/4 v0, 0x1

    sput-boolean v0, Lcom/kamcord/android/Kamcord;->j:Z

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    if-eqz v0, :cond_0

    invoke-interface {v0}, Landroid/content/SharedPreferences;->edit()Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    const-string v1, "voice_overlay_enabled"

    invoke-interface {v0, v1, p0}, Landroid/content/SharedPreferences$Editor;->putBoolean(Ljava/lang/String;Z)Landroid/content/SharedPreferences$Editor;

    move-result-object v0

    invoke-interface {v0}, Landroid/content/SharedPreferences$Editor;->commit()Z

    :cond_0
    return-void
.end method

.method public static setVoiceOverlayEnabled(Z)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    sput-boolean p0, Lcom/kamcord/android/Kamcord;->h:Z

    :cond_0
    return-void
.end method

.method public static showView()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_1

    sget-object v1, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-object v2, Lcom/kamcord/android/KC_o;->a:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_o;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->k()V

    sget-object v0, Lcom/kamcord/android/KC_o;->d:Lcom/kamcord/android/KC_o;

    sput-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    :goto_0
    monitor-exit v1

    :goto_1
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "showView called with state:"

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget-object v2, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const/4 v0, 0x0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->notifyViewDidNotAppear(Z)V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_1
    const/4 v0, 0x1

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->notifyViewDidNotAppear(Z)V

    goto :goto_1
.end method

.method public static showWatchView()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->b()Z

    move-result v0

    if-eqz v0, :cond_1

    sget-object v1, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-object v2, Lcom/kamcord/android/KC_o;->a:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_o;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->l()V

    sget-object v0, Lcom/kamcord/android/KC_o;->d:Lcom/kamcord/android/KC_o;

    sput-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    :goto_0
    monitor-exit v1

    :goto_1
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "showWatchView called with state:"

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget-object v2, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const/4 v0, 0x0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->notifyViewDidNotAppear(Z)V
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_1
    const/4 v0, 0x1

    invoke-static {v0}, Lcom/kamcord/android/Kamcord;->notifyViewDidNotAppear(Z)V

    goto :goto_1
.end method

.method public static startRecording()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_1

    sget-object v1, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-object v2, Lcom/kamcord/android/KC_o;->a:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_o;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_0

    const-string v0, "startRecording sent."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->g()V

    sget-object v0, Lcom/kamcord/android/KC_o;->b:Lcom/kamcord/android/KC_o;

    sput-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    :goto_0
    monitor-exit v1

    :goto_1
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "startRecording called with state:"

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget-object v2, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_1
    const-string v0, "startRecording called with Kamcord disabled."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    goto :goto_1
.end method

.method public static stopRecording()V
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_2

    sget-object v1, Lcom/kamcord/android/Kamcord;->c:Ljava/lang/Object;

    monitor-enter v1

    :try_start_0
    sget-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-object v2, Lcom/kamcord/android/KC_o;->b:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_o;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-nez v0, :cond_0

    sget-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    sget-object v2, Lcom/kamcord/android/KC_o;->c:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Lcom/kamcord/android/KC_o;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_1

    :cond_0
    const-string v0, "stopRecording called."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/core/KC_u;->h()V

    sget-object v0, Lcom/kamcord/android/KC_o;->a:Lcom/kamcord/android/KC_o;

    sput-object v0, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    :goto_0
    monitor-exit v1

    :goto_1
    return-void

    :cond_1
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "stopRecording called with state:"

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget-object v2, Lcom/kamcord/android/Kamcord;->d:Lcom/kamcord/android/KC_o;

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/Object;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I
    :try_end_0
    .catchall {:try_start_0 .. :try_end_0} :catchall_0

    goto :goto_0

    :catchall_0
    move-exception v0

    monitor-exit v1

    throw v0

    :cond_2
    const-string v0, "stopRecording called with Kamcord disabled."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    goto :goto_1
.end method

.method public static usingUnity()Z
    .locals 1

    sget-object v0, Lcom/kamcord/android/Kamcord;->g:Lcom/kamcord/android/KC_V;

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static version()Ljava/lang/String;
    .locals 1

    const-string v0, "1.4.7"

    return-object v0
.end method

.method public static videoIncompleteWarningEnabled()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->k:Z

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static voiceOverlayActivated()Z
    .locals 3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getSharedPreferences()Landroid/content/SharedPreferences;

    move-result-object v0

    if-eqz v0, :cond_0

    const-string v1, "voice_overlay_enabled"

    invoke-interface {v0, v1}, Landroid/content/SharedPreferences;->contains(Ljava/lang/String;)Z

    move-result v1

    if-eqz v1, :cond_0

    const-string v1, "voice_overlay_enabled"

    const/4 v2, 0x0

    invoke-interface {v0, v1, v2}, Landroid/content/SharedPreferences;->getBoolean(Ljava/lang/String;Z)Z

    move-result v0

    :goto_0
    return v0

    :cond_0
    sget-boolean v0, Lcom/kamcord/android/Kamcord;->i:Z

    goto :goto_0
.end method

.method public static voiceOverlayEnabled()Z
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_a;->o()Z

    move-result v0

    if-eqz v0, :cond_0

    sget-boolean v0, Lcom/kamcord/android/Kamcord;->h:Z

    if-eqz v0, :cond_0

    const/4 v0, 0x1

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static whitelistAll()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    invoke-static {}, Lcom/kamcord/android/a/KC_c;->a()V

    goto :goto_0
.end method

.method public static whitelistAllBoards()V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    const-string v0, "whitelistAllBoards() is deprecated. Use whitelistAll() instead."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->a()V

    goto :goto_0
.end method

.method public static whitelistBoard(Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Whitelisting "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {p0}, Lcom/kamcord/android/a/KC_c;->a(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public static whitelistBoard(Ljava/lang/String;I)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Whitelisting "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " for sdk version "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {p0, p1}, Lcom/kamcord/android/a/KC_c;->a(Ljava/lang/String;I)V

    goto :goto_0
.end method

.method public static whitelistDevice(Ljava/lang/String;)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Whitelisting "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {p0}, Lcom/kamcord/android/a/KC_c;->c(Ljava/lang/String;)V

    goto :goto_0
.end method

.method public static whitelistDevice(Ljava/lang/String;I)V
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->k()Z

    move-result v0

    if-eqz v0, :cond_0

    :goto_0
    return-void

    :cond_0
    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Whitelisting "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " for sdk version "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {p0, p1}, Lcom/kamcord/android/a/KC_c;->c(Ljava/lang/String;I)V

    goto :goto_0
.end method

.method public static writeAudioData([FI)V
    .locals 1

    invoke-static {}, Lcom/kamcord/android/Kamcord;->isEnabled()Z

    move-result v0

    if-eqz v0, :cond_0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->d()Lcom/kamcord/android/Kamcord;

    invoke-static {}, Lcom/kamcord/android/Kamcord;->e()Lcom/kamcord/android/core/KC_u;

    invoke-static {p0, p1}, Lcom/kamcord/android/core/KC_u;->a([FI)V

    :cond_0
    return-void
.end method
