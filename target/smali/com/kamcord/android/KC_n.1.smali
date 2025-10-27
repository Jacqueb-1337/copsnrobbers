.class public final Lcom/kamcord/android/KC_n;
.super Ljava/lang/Object;

# interfaces
.implements Landroid/content/ComponentCallbacks;


# static fields
.field private static a:Lcom/kamcord/android/KC_n;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    const/4 v0, 0x0

    sput-object v0, Lcom/kamcord/android/KC_n;->a:Lcom/kamcord/android/KC_n;

    return-void
.end method

.method private constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method

.method public static a()Lcom/kamcord/android/KC_n;
    .locals 1

    sget-object v0, Lcom/kamcord/android/KC_n;->a:Lcom/kamcord/android/KC_n;

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/KC_n;

    invoke-direct {v0}, Lcom/kamcord/android/KC_n;-><init>()V

    sput-object v0, Lcom/kamcord/android/KC_n;->a:Lcom/kamcord/android/KC_n;

    :cond_0
    sget-object v0, Lcom/kamcord/android/KC_n;->a:Lcom/kamcord/android/KC_n;

    return-object v0
.end method


# virtual methods
.method public final onConfigurationChanged(Landroid/content/res/Configuration;)V
    .locals 0

    return-void
.end method

.method public final onLowMemory()V
    .locals 1

    sget-boolean v0, Lcom/kamcord/android/core/KC_u;->c:Z

    if-nez v0, :cond_0

    const-string v0, "Running low on memory! Shutting down Kamcord..."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/Kamcord;->bail()V

    :cond_0
    return-void
.end method
