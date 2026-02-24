.class public abstract Lcom/kamcord/a/a/a/a/KC_c;
.super Ljava/lang/Object;

# interfaces
.implements Lcom/kamcord/a/a/a/a/KC_a;


# direct methods
.method public constructor <init>()V
    .locals 0

    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public a()Lcom/kamcord/a/a/c/KC_a;
    .locals 1

    new-instance v0, Lcom/kamcord/a/a/c/KC_d;

    invoke-direct {v0}, Lcom/kamcord/a/a/c/KC_d;-><init>()V

    return-object v0
.end method

.method public a(Lcom/kamcord/a/a/d/KC_a;)Lcom/kamcord/a/a/e/KC_c;
    .locals 1

    new-instance v0, Lcom/kamcord/a/a/e/KC_b;

    invoke-direct {v0, p0, p1}, Lcom/kamcord/a/a/e/KC_b;-><init>(Lcom/kamcord/a/a/a/a/KC_c;Lcom/kamcord/a/a/d/KC_a;)V

    return-object v0
.end method

.method public b()Lcom/kamcord/a/a/d/KC_k;
    .locals 1

    sget-object v0, Lcom/kamcord/a/a/d/KC_k;->a:Lcom/kamcord/a/a/d/KC_k;

    return-object v0
.end method

.method public abstract b(Lcom/kamcord/a/a/d/KC_a;)Ljava/lang/String;
.end method

.method public abstract c()Ljava/lang/String;
.end method
