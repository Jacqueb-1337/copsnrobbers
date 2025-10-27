.class public final Lcom/kamcord/android/a/KC_c;
.super Ljava/lang/Object;


# static fields
.field private static a:Lcom/kamcord/android/a/KC_a;

.field private static b:Z

.field private static c:Z

.field private static d:Lcom/kamcord/android/a/KC_d;

.field private static e:Lcom/kamcord/android/a/KC_g;


# direct methods
.method public static a()V
    .locals 1

    const/4 v0, 0x1

    sput-boolean v0, Lcom/kamcord/android/a/KC_c;->b:Z

    return-void
.end method

.method public static a(Ljava/lang/String;)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "whitelisting board "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v0

    invoke-virtual {v0, p0}, Lcom/kamcord/android/a/KC_g;->c(Ljava/lang/String;)V

    return-void
.end method

.method public static a(Ljava/lang/String;I)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "whitelisting board "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " on version "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v0

    invoke-virtual {v0, p0, p1}, Lcom/kamcord/android/a/KC_g;->c(Ljava/lang/String;I)V

    return-void
.end method

.method public static b()V
    .locals 1

    const/4 v0, 0x1

    sput-boolean v0, Lcom/kamcord/android/a/KC_c;->c:Z

    const/4 v0, 0x0

    sput-boolean v0, Lcom/kamcord/android/a/KC_c;->b:Z

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_g;->a()V

    return-void
.end method

.method public static b(Ljava/lang/String;)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "blacklisting board "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v0

    invoke-virtual {v0, p0}, Lcom/kamcord/android/a/KC_g;->c(Ljava/lang/String;)V

    return-void
.end method

.method public static b(Ljava/lang/String;I)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "blacklisting board "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " on version "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v0

    invoke-virtual {v0, p0, p1}, Lcom/kamcord/android/a/KC_g;->c(Ljava/lang/String;I)V

    return-void
.end method

.method public static c(Ljava/lang/String;)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "whitelisting device "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v0

    invoke-virtual {v0, p0}, Lcom/kamcord/android/a/KC_g;->a(Ljava/lang/String;)V

    return-void
.end method

.method public static c(Ljava/lang/String;I)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "whitelisting device "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " on version "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v0

    invoke-virtual {v0, p0, p1}, Lcom/kamcord/android/a/KC_g;->a(Ljava/lang/String;I)V

    return-void
.end method

.method public static c()Z
    .locals 3

    const/4 v0, 0x1

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v1

    invoke-virtual {v1}, Lcom/kamcord/android/a/KC_g;->b()Lcom/kamcord/android/a/KC_h;

    move-result-object v1

    sget-boolean v2, Lcom/kamcord/android/a/KC_c;->b:Z

    if-eqz v2, :cond_0

    if-nez v1, :cond_0

    const-string v1, "All devices are whitelisted."

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    :goto_0
    return v0

    :cond_0
    if-eqz v1, :cond_1

    const-string v0, "device or board matches whitelist entry: "

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "board: "

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, v1, Lcom/kamcord/android/a/KC_h;->a:Ljava/lang/String;

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "device: "

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-object v2, v1, Lcom/kamcord/android/a/KC_h;->b:Ljava/lang/String;

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "sdkVersion: "

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget v2, v1, Lcom/kamcord/android/a/KC_h;->c:I

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v2, "white: "

    invoke-direct {v0, v2}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    iget-boolean v2, v1, Lcom/kamcord/android/a/KC_h;->d:Z

    invoke-virtual {v0, v2}, Ljava/lang/StringBuilder;->append(Z)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    iget-boolean v0, v1, Lcom/kamcord/android/a/KC_h;->d:Z

    goto :goto_0

    :cond_1
    sget-boolean v1, Lcom/kamcord/android/a/KC_c;->c:Z

    if-nez v1, :cond_2

    const-string v1, "whitelist entry not found, looking in device info list. "

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->d()Lcom/kamcord/android/a/KC_a;

    move-result-object v1

    instance-of v1, v1, Lcom/kamcord/android/a/KC_e;

    if-eqz v1, :cond_2

    const-string v1, "Device found in info list, whitelisted!"

    invoke-static {v1}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    goto :goto_0

    :cond_2
    const-string v0, "Blacklisted by default."

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    const/4 v0, 0x0

    goto :goto_0
.end method

.method public static d()Lcom/kamcord/android/a/KC_a;
    .locals 5

    sget-object v0, Lcom/kamcord/android/a/KC_c;->a:Lcom/kamcord/android/a/KC_a;

    if-nez v0, :cond_0

    sget-object v0, Landroid/os/Build;->DEVICE:Ljava/lang/String;

    sget v1, Landroid/os/Build$VERSION;->SDK_INT:I

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Device = "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getDevice()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Board = "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getBoard()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "Android SDK = "

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    sget v3, Landroid/os/Build$VERSION;->SDK_INT:I

    invoke-virtual {v2, v3}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-static {v2}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->f()V

    sget-object v2, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    iget-object v2, v2, Lcom/kamcord/android/a/KC_d;->a:Ljava/util/HashMap;

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v3, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "@"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/util/HashMap;->containsKey(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_1

    new-instance v2, Lcom/kamcord/android/a/KC_e;

    sget-object v3, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    iget-object v3, v3, Lcom/kamcord/android/a/KC_d;->a:Ljava/util/HashMap;

    new-instance v4, Ljava/lang/StringBuilder;

    invoke-direct {v4}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v4, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v4, "@"

    invoke-virtual {v0, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v3, v0}, Ljava/util/HashMap;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/a/KC_b;

    invoke-direct {v2, v0}, Lcom/kamcord/android/a/KC_e;-><init>(Lcom/kamcord/android/a/KC_b;)V

    sput-object v2, Lcom/kamcord/android/a/KC_c;->a:Lcom/kamcord/android/a/KC_a;

    :cond_0
    :goto_0
    sget-object v0, Lcom/kamcord/android/a/KC_c;->a:Lcom/kamcord/android/a/KC_a;

    return-object v0

    :cond_1
    sget-object v2, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    iget-object v2, v2, Lcom/kamcord/android/a/KC_d;->a:Ljava/util/HashMap;

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v3, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    const-string v4, "@*"

    invoke-virtual {v3, v4}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v3

    invoke-virtual {v3}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v3

    invoke-virtual {v2, v3}, Ljava/util/HashMap;->containsKey(Ljava/lang/Object;)Z

    move-result v2

    if-eqz v2, :cond_2

    new-instance v1, Lcom/kamcord/android/a/KC_e;

    sget-object v2, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    iget-object v2, v2, Lcom/kamcord/android/a/KC_d;->a:Ljava/util/HashMap;

    new-instance v3, Ljava/lang/StringBuilder;

    invoke-direct {v3}, Ljava/lang/StringBuilder;-><init>()V

    invoke-virtual {v3, v0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v3, "@*"

    invoke-virtual {v0, v3}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-virtual {v2, v0}, Ljava/util/HashMap;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/a/KC_b;

    invoke-direct {v1, v0}, Lcom/kamcord/android/a/KC_e;-><init>(Lcom/kamcord/android/a/KC_b;)V

    sput-object v1, Lcom/kamcord/android/a/KC_c;->a:Lcom/kamcord/android/a/KC_a;

    goto :goto_0

    :cond_2
    sget-object v0, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    iget-object v0, v0, Lcom/kamcord/android/a/KC_d;->a:Ljava/util/HashMap;

    new-instance v2, Ljava/lang/StringBuilder;

    const-string v3, "*@"

    invoke-direct {v2, v3}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v2, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v2

    invoke-virtual {v2}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v2

    invoke-virtual {v0, v2}, Ljava/util/HashMap;->containsKey(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_3

    new-instance v2, Lcom/kamcord/android/a/KC_e;

    sget-object v0, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    iget-object v0, v0, Lcom/kamcord/android/a/KC_d;->a:Ljava/util/HashMap;

    new-instance v3, Ljava/lang/StringBuilder;

    const-string v4, "*@"

    invoke-direct {v3, v4}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v3, v1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v1

    invoke-virtual {v1}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v1

    invoke-virtual {v0, v1}, Ljava/util/HashMap;->get(Ljava/lang/Object;)Ljava/lang/Object;

    move-result-object v0

    check-cast v0, Lcom/kamcord/android/a/KC_b;

    invoke-direct {v2, v0}, Lcom/kamcord/android/a/KC_e;-><init>(Lcom/kamcord/android/a/KC_b;)V

    sput-object v2, Lcom/kamcord/android/a/KC_c;->a:Lcom/kamcord/android/a/KC_a;

    goto :goto_0

    :cond_3
    new-instance v0, Lcom/kamcord/android/a/KC_f;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_f;-><init>()V

    sput-object v0, Lcom/kamcord/android/a/KC_c;->a:Lcom/kamcord/android/a/KC_a;

    goto/16 :goto_0
.end method

.method public static d(Ljava/lang/String;)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "blacklisting device "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v0

    invoke-virtual {v0, p0}, Lcom/kamcord/android/a/KC_g;->b(Ljava/lang/String;)V

    return-void
.end method

.method public static d(Ljava/lang/String;I)V
    .locals 2

    new-instance v0, Ljava/lang/StringBuilder;

    const-string v1, "blacklisting device "

    invoke-direct {v0, v1}, Ljava/lang/StringBuilder;-><init>(Ljava/lang/String;)V

    invoke-virtual {v0, p0}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    const-string v1, " on version "

    invoke-virtual {v0, v1}, Ljava/lang/StringBuilder;->append(Ljava/lang/String;)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0, p1}, Ljava/lang/StringBuilder;->append(I)Ljava/lang/StringBuilder;

    move-result-object v0

    invoke-virtual {v0}, Ljava/lang/StringBuilder;->toString()Ljava/lang/String;

    move-result-object v0

    invoke-static {v0}, Lcom/kamcord/android/Kamcord$KC_a;->a(Ljava/lang/String;)I

    invoke-static {}, Lcom/kamcord/android/a/KC_c;->e()Lcom/kamcord/android/a/KC_g;

    move-result-object v0

    invoke-virtual {v0, p0, p1}, Lcom/kamcord/android/a/KC_g;->b(Ljava/lang/String;I)V

    return-void
.end method

.method private static e()Lcom/kamcord/android/a/KC_g;
    .locals 1

    sget-object v0, Lcom/kamcord/android/a/KC_c;->e:Lcom/kamcord/android/a/KC_g;

    if-nez v0, :cond_0

    new-instance v0, Lcom/kamcord/android/a/KC_g;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_g;-><init>()V

    sput-object v0, Lcom/kamcord/android/a/KC_c;->e:Lcom/kamcord/android/a/KC_g;

    :cond_0
    sget-object v0, Lcom/kamcord/android/a/KC_c;->e:Lcom/kamcord/android/a/KC_g;

    return-object v0
.end method

.method private static f()V
    .locals 13

    const/4 v12, 0x4

    const/16 v11, 0x300

    const/16 v10, 0x2d0

    const/16 v9, 0x15

    const/16 v8, 0x500

    new-instance v0, Lcom/kamcord/android/a/KC_d;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_d;-><init>()V

    sput-object v0, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const/16 v0, 0x1f

    new-array v1, v0, [Ljava/lang/String;

    const/4 v0, 0x0

    const-string v2, "c1att"

    aput-object v2, v1, v0

    const/4 v0, 0x1

    const-string v2, "c1ktt"

    aput-object v2, v1, v0

    const/4 v0, 0x2

    const-string v2, "c1lgt"

    aput-object v2, v1, v0

    const/4 v0, 0x3

    const-string v2, "c1skt"

    aput-object v2, v1, v0

    const-string v0, "d2att"

    aput-object v0, v1, v12

    const/4 v0, 0x5

    const-string v2, "d2can"

    aput-object v2, v1, v0

    const/4 v0, 0x6

    const-string v2, "d2cri"

    aput-object v2, v1, v0

    const/4 v0, 0x7

    const-string v2, "d2dcm"

    aput-object v2, v1, v0

    const/16 v0, 0x8

    const-string v2, "d2lteMetroPCS"

    aput-object v2, v1, v0

    const/16 v0, 0x9

    const-string v2, "d2ltetmo"

    aput-object v2, v1, v0

    const/16 v0, 0xa

    const-string v2, "d2mtr"

    aput-object v2, v1, v0

    const/16 v0, 0xb

    const-string v2, "d2spi"

    aput-object v2, v1, v0

    const/16 v0, 0xc

    const-string v2, "d2spr"

    aput-object v2, v1, v0

    const/16 v0, 0xd

    const-string v2, "d2tfnspr"

    aput-object v2, v1, v0

    const/16 v0, 0xe

    const-string v2, "d2tfnvzw"

    aput-object v2, v1, v0

    const/16 v0, 0xf

    const-string v2, "d2tmo"

    aput-object v2, v1, v0

    const/16 v0, 0x10

    const-string v2, "d2usc"

    aput-object v2, v1, v0

    const/16 v0, 0x11

    const-string v2, "d2vmu"

    aput-object v2, v1, v0

    const/16 v0, 0x12

    const-string v2, "d2vzw"

    aput-object v2, v1, v0

    const/16 v0, 0x13

    const-string v2, "d2xar"

    aput-object v2, v1, v0

    const/16 v0, 0x14

    const-string v2, "m0"

    aput-object v2, v1, v0

    const-string v0, "m0apt"

    aput-object v0, v1, v9

    const/16 v0, 0x16

    const-string v2, "m0chn"

    aput-object v2, v1, v0

    const/16 v0, 0x17

    const-string v2, "m0cmcc"

    aput-object v2, v1, v0

    const/16 v0, 0x18

    const-string v2, "m0ctc"

    aput-object v2, v1, v0

    const/16 v0, 0x19

    const-string v2, "m0ctcduos"

    aput-object v2, v1, v0

    const/16 v0, 0x1a

    const-string v2, "m0skt"

    aput-object v2, v1, v0

    const/16 v0, 0x1b

    const-string v2, "m3"

    aput-object v2, v1, v0

    const/16 v0, 0x1c

    const-string v2, "m3dcm"

    aput-object v2, v1, v0

    const/16 v0, 0x1d

    const-string v2, "SC-03E"

    aput-object v2, v1, v0

    const/16 v0, 0x1e

    const-string v2, "arubaslim"

    aput-object v2, v1, v0

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getBoard()Ljava/lang/String;

    move-result-object v0

    const-string v2, "msm8960"

    invoke-virtual {v0, v2}, Ljava/lang/String;->equals(Ljava/lang/Object;)Z

    move-result v0

    if-eqz v0, :cond_0

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v2, "Galaxy S3"

    invoke-virtual {v0, v2}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v2, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v2, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v2}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v2, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v2}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v2, "video/avc"

    invoke-virtual {v0, v2}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v2

    array-length v3, v1

    const/4 v0, 0x0

    :goto_0
    if-ge v0, v3, :cond_1

    aget-object v4, v1, v0

    sget-object v5, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    invoke-virtual {v5, v4, v2}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    add-int/lit8 v0, v0, 0x1

    goto :goto_0

    :cond_0
    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v2, "Galaxy S3"

    invoke-virtual {v0, v2}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v2, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v2, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v2}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v2, "OMX.SEC.AVC.Encoder"

    invoke-virtual {v0, v2}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v2, "video/avc"

    invoke-virtual {v0, v2}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v2, 0x0

    invoke-virtual {v0, v2}, Lcom/kamcord/android/a/KC_b$KC_a;->e(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v2

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v3, "Galaxy S3"

    invoke-virtual {v0, v3}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v3, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v3, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v3}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v3, "OMX.SEC.AVC.Encoder"

    invoke-virtual {v0, v3}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v3, "video/avc"

    invoke-virtual {v0, v3}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v3, 0x0

    invoke-virtual {v0, v3}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v3, 0x0

    invoke-virtual {v0, v3}, Lcom/kamcord/android/a/KC_b$KC_a;->e(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v3

    array-length v4, v1

    const/4 v0, 0x0

    :goto_1
    if-ge v0, v4, :cond_1

    aget-object v5, v1, v0

    sget-object v6, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const/16 v7, 0x10

    invoke-virtual {v6, v5, v7, v2}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;ILcom/kamcord/android/a/KC_b;)V

    sget-object v6, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const/16 v7, 0x11

    invoke-virtual {v6, v5, v7, v2}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;ILcom/kamcord/android/a/KC_b;)V

    sget-object v6, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    invoke-virtual {v6, v5, v3}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    add-int/lit8 v0, v0, 0x1

    goto :goto_1

    :cond_1
    invoke-static {}, Lcom/kamcord/android/Kamcord;->getBoard()Ljava/lang/String;

    move-result-object v0

    const-string v1, "msm"

    invoke-virtual {v0, v1}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result v0

    if-nez v0, :cond_2

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getBoard()Ljava/lang/String;

    move-result-object v0

    const-string v1, "apq"

    invoke-virtual {v0, v1}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_4

    :cond_2
    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Galaxy S4"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    :goto_2
    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ja3g"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ja3gduosctc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ja3g"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ja3gduosctc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jaltektt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jaltelgt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jalteskt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jflte"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jflteaio"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jflteatt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfltecan"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfltecri"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfltecsp"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfltelra"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jflterefreshspr"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfltespr"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfltetfntmo"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfltetmo"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jflteusc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfltevzw"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jftdd"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfvelte"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jfwifi"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ks01lte"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ks01ltektt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ks01ltelgt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ks01lteskt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SC-04E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jactivelte"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jactivelteatt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jactivelteskt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ja3gchnduos"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "jgedlte"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getBoard()Ljava/lang/String;

    move-result-object v0

    const-string v1, "msm"

    invoke-virtual {v0, v1}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result v0

    if-nez v0, :cond_3

    invoke-static {}, Lcom/kamcord/android/Kamcord;->getBoard()Ljava/lang/String;

    move-result-object v0

    const-string v1, "apq"

    invoke-virtual {v0, v1}, Ljava/lang/String;->startsWith(Ljava/lang/String;)Z

    move-result v0

    if-eqz v0, :cond_5

    :cond_3
    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Galaxy S5"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    :goto_3
    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "k3g"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "klte"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "klteacg"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "klteatt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "kltecan"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "kltektt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "kltelgt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "kltelra"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "klteMetroPCS"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "klteskt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "kltespr"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "kltetmo"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "klteusc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "kltevzw"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "kwifi"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SC-04F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SCL23"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "klteattactive"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Samsung Galaxy Mega 6.3"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "melius3g"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "melius3gduosctc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "meliuslte"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "meliuslteatt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "meliusltecan"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "meliusltektt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "meliusltelgt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "meliuslteMetroPCS"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "meliuslteskt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "meliusltespr"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "meliuslteusc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Nexus 4"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x6

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "mako"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Nexus 5"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hammerhead"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Nexus 7"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    const/16 v2, 0x320

    invoke-direct {v1, v2, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x5

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "grouper"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "tilapia"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Nexus 7 HD"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    const/16 v2, 0x320

    invoke-direct {v1, v2, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x5

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "flo"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "deb"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Nexus 10"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    const/16 v2, 0x320

    invoke-direct {v1, v2, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "manta"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "HTC One M7"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "m7"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "m7cdtu"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "m7cdug"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "m7cdwg"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "m7wls"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "m7wlv"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "HTC One Max"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t6ul"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t6wl"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t6whl"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "HTC One M8"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "htc_m8"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "htc_m8dug"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "htc_m8whl"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "htc_m8wl"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "HTC One X+"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.google.aac.encoder"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "evitareul"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "enrc2b"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "HTC J One"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "m7wlj"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "HTC J Butterfly"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "dlxj"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "HTC Butterfly"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "dlxu"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "dlxub1"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "HTC Droid DNA"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "dlx"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "INFOBAR A02"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "imnj"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "LG isai LGL22 / LG G2"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "g2"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "LG G Flex"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "zee"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "LG G Optimus"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "geeb"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "geehdc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "geehrc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "geehrc4g"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "LG Optimus G Pro"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "geefhd"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "geefhd4g"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "geevl04e"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "gvarfhd"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "LG Optimus it L-05E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "L05E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE SERIE SHL21 Pro"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SHL21"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE SERIE SHL22"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SHL22"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE SERIE SHL23"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SHL23"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE SERIE mini SHL24"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SHL24"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE SERIE SHL25"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SHL25"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE EX SH-02F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH-02F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE EX SH-04E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH04E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE si SH-07E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH-07E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PAD SH-06F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    const/16 v2, 0x320

    invoke-direct {v1, v2, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH-06F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PAD SHT22"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    const/16 v2, 0x320

    invoke-direct {v1, v2, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SHT22"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS Xx 304SH"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SG304SH"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE Xx mini 303SH"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM303SH"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE Xx mini 302SH"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM302SH"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE Xx 203SH"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM203SH"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE ZETA SH-01F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH-01F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH-01FDQ"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE ZETA SH-06E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH-06E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Disney Mobile on docomo SH-05F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH-05F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Disney Mobile on docomo F-07E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F07E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Disney Mobile on docomo F-03F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F03F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS NX F-01F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F01F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS NX F-05F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F05F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS NX F-06E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F06E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS X F-02E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F02E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS A SoftBank 202F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM202F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS A SoftBank 301F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM301F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS Z FJL22"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "FJL22_jp_kdi"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Disney Mobile DM015K"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "DM015K"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Kyocera Digno M"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "KYL22"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Kyocera Digno R"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "202K"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "URBANO L02"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "KYY22"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "URBANO L01"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "KYY21"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "NEC Medias X N-06E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "N-06E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "NEC Medias X N-04E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "N-04E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ELUGA P"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "P-03E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ELUGA X P-02"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "P-02E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia A"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-04E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia AX"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-01E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia A2"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-04F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia Z"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6602"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6603"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6606"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6616"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "L36h"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-02E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia Z1"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6902"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6903"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6906"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6943"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "L39h"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "L39t"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "L39u"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-01F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SOL23"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia Z2"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-03F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "D6502"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "D6503"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "D6543"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia Z2 Tablet"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    const/16 v2, 0x320

    invoke-direct {v1, v2, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP511"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP512"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP521"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP551"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-05F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia Z1f"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-02F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia Z Ultra"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6802"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6806"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6833"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6843"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP412"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SOL24"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "XL39h"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia UL"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SOL22"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia SP"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C5302"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C5303"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C5306"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "M35h"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "M35t"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "M35c"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia V"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "LT25i"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "LT25c"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia ZL"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6502"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6503"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "C6506"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia ZL2"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SOL25"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Motorola Moto X"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ghost"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Motorola Moto G"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "falcon_cdma"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "falcon_umts"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "falcon_umtsds"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Motorola Droid Mini"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "obakem"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Motorola Droid Maxx"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "obake-maxx"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Pantech VEGA PTL21"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "maruko"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xiaomi MI 2"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "aries"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Huawei Media Pad X"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    const/16 v2, 0x180

    const/16 v3, 0x280

    invoke-direct {v1, v2, v3}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.k3.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x1

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hw7d501l"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Huawei"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hwH60"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Galaxy Note 2"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.SEC.AVC.Encoder"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const v1, 0x7fffffff

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SC-02E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t03g"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t03gchn"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t03gchnduos"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t03gcmcc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t03gctc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t03gcuduos"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0lte"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0lteatt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0ltecan"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0ltecmcc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0ltedcm"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0ltektt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0ltelgt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0lteskt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0ltespr"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0ltetmo"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0lteusc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "t0ltevzw"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Galaxy Note 3"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "ha3g"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hlte"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hlteatt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hltecan"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hltektt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hltelgt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hlteskt"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hltespr"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hltetmo"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hlteusc"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "hltevzw"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SC-02F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SC-01F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SCL22"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE sv SH-10D"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH10D"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia GX SO-04D"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-04D"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE ss 205SH"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM205SH"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia Tablet Z"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-03E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP311"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP312"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP321"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP341"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SGP351"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS A SoftBank 101F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM101F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE Xx 206SH"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM206SH"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "PANTONE 6 200SH"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM200SH"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Disney mobile DMO16SH"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "DM016SH"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE si SH-01E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH01E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS A SoftBank 201F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SBM201F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia SX"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SO-05D"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Xperia VL"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SOL21"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "MEDIAS W N-05E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "N-05E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PHONE ZETA SH-02E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH02E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Disney Mobile on docomo N-03E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "N-03E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Galaxy Note"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SC-05D"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS Tab F-05E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/16 v1, 0x13

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F05E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "AQUOS PAD SH-08E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH-08E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS Tab F-02F"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F02F"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "ARROWS Tab FJT21"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "FJT21_jp_kdi"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "F-10D"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.Nvidia.h264.encoder"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F10D"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "L-05D"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "l_dcm"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "F-04E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.Nvidia.h264.encoder"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/16 v1, 0x13

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "F04E"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "WX10K"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "WX10K"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "WX04SH"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "WX04SH"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "SH-09D"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SH09D"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "T-02D"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "T02D"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "scl21"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "SCL21"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Medias X N-07D"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "N-07D"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "FJL21"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "FJL21"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "L-02E"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v11, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.qcom.video.encoder.avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    sget-object v1, Lcom/kamcord/android/a/KC_c;->d:Lcom/kamcord/android/a/KC_d;

    const-string v2, "l1_dcm"

    invoke-virtual {v1, v2, v0}, Lcom/kamcord/android/a/KC_d;->a(Ljava/lang/String;Lcom/kamcord/android/a/KC_b;)V

    return-void

    :cond_4
    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Galaxy S4"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.Exynos.AVC.Encoder"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->e(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    goto/16 :goto_2

    :cond_5
    new-instance v0, Lcom/kamcord/android/a/KC_b$KC_a;

    invoke-direct {v0}, Lcom/kamcord/android/a/KC_b$KC_a;-><init>()V

    const-string v1, "Galaxy S5"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->d(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    new-instance v1, Lcom/kamcord/android/core/KC_f;

    invoke-direct {v1, v10, v8}, Lcom/kamcord/android/core/KC_f;-><init>(II)V

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->a(Lcom/kamcord/android/core/KC_f;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "OMX.Exynos.AVC.Encoder"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->b(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const-string v1, "video/avc"

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->c(Ljava/lang/String;)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v9}, Lcom/kamcord/android/a/KC_b$KC_a;->a(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0, v12}, Lcom/kamcord/android/a/KC_b$KC_a;->b(I)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    const/4 v1, 0x0

    invoke-virtual {v0, v1}, Lcom/kamcord/android/a/KC_b$KC_a;->e(Z)Lcom/kamcord/android/a/KC_b$KC_a;

    move-result-object v0

    invoke-virtual {v0}, Lcom/kamcord/android/a/KC_b$KC_a;->a()Lcom/kamcord/android/a/KC_b;

    move-result-object v0

    goto/16 :goto_3
.end method
