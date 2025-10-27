.class public final Lcom/kamcord/android/Kamcord$KC_a;
.super Ljava/lang/Object;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = Lcom/kamcord/android/Kamcord;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x9
    name = "KC_a"
.end annotation


# static fields
.field private static a:Ljava/lang/String;


# direct methods
.method static constructor <clinit>()V
    .locals 1

    const-string v0, "Kamcord"

    sput-object v0, Lcom/kamcord/android/Kamcord$KC_a;->a:Ljava/lang/String;

    return-void
.end method

.method private static a(ILjava/lang/String;Ljava/lang/String;)I
    .locals 2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->c()I

    move-result v0

    if-gt v0, p0, :cond_0

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "Kamcord: "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {p0, p1, v0}, Landroid/util/Log;->println(ILjava/lang/String;Ljava/lang/String;)I

    move-result v0

    :goto_0
    return v0

    :cond_0
    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static a(Ljava/lang/String;)I
    .locals 2

    const/4 v0, 0x2

    sget-object v1, Lcom/kamcord/android/Kamcord$KC_a;->a:Ljava/lang/String;

    invoke-static {v0, v1, p0}, Lcom/kamcord/android/Kamcord$KC_a;->a(ILjava/lang/String;Ljava/lang/String;)I

    move-result v0

    return v0
.end method

.method public static a(Ljava/lang/String;Ljava/lang/String;)I
    .locals 1

    const/4 v0, 0x2

    invoke-static {v0, p0, p1}, Lcom/kamcord/android/Kamcord$KC_a;->a(ILjava/lang/String;Ljava/lang/String;)I

    move-result v0

    return v0
.end method

.method public static b(Ljava/lang/String;)I
    .locals 2

    const/4 v0, 0x4

    sget-object v1, Lcom/kamcord/android/Kamcord$KC_a;->a:Ljava/lang/String;

    invoke-static {v0, v1, p0}, Lcom/kamcord/android/Kamcord$KC_a;->a(ILjava/lang/String;Ljava/lang/String;)I

    move-result v0

    return v0
.end method

.method public static b(Ljava/lang/String;Ljava/lang/String;)I
    .locals 1

    const/4 v0, 0x5

    invoke-static {v0, p0, p1}, Lcom/kamcord/android/Kamcord$KC_a;->a(ILjava/lang/String;Ljava/lang/String;)I

    move-result v0

    return v0
.end method

.method public static c(Ljava/lang/String;)I
    .locals 2

    const/4 v0, 0x5

    sget-object v1, Lcom/kamcord/android/Kamcord$KC_a;->a:Ljava/lang/String;

    invoke-static {v0, v1, p0}, Lcom/kamcord/android/Kamcord$KC_a;->a(ILjava/lang/String;Ljava/lang/String;)I

    move-result v0

    return v0
.end method

.method public static c(Ljava/lang/String;Ljava/lang/String;)I
    .locals 1

    const/4 v0, 0x6

    invoke-static {v0, p0, p1}, Lcom/kamcord/android/Kamcord$KC_a;->a(ILjava/lang/String;Ljava/lang/String;)I

    move-result v0

    return v0
.end method

.method public static d(Ljava/lang/String;)I
    .locals 2

    const/4 v0, 0x6

    sget-object v1, Lcom/kamcord/android/Kamcord$KC_a;->a:Ljava/lang/String;

    invoke-static {v0, v1, p0}, Lcom/kamcord/android/Kamcord$KC_a;->a(ILjava/lang/String;Ljava/lang/String;)I

    move-result v0

    return v0
.end method
