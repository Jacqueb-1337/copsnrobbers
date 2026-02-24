.class final La/a/a/e/a/KC_c$KC_b;
.super La/a/a/e/a/KC_b;
.source "SourceFile"


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/a/KC_c;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = "KC_b"
.end annotation


# direct methods
.method constructor <init>()V
    .locals 0

    .prologue
    .line 84
    invoke-direct {p0}, La/a/a/e/a/KC_b;-><init>()V

    return-void
.end method


# virtual methods
.method public final a(La/a/a/e/a/KC_c;)Ljava/lang/Object;
    .locals 2

    .prologue
    .line 88
    new-instance v0, La/a/a/e/a/KC_c$KC_b$KC_a;

    invoke-direct {v0, p0, p1}, La/a/a/e/a/KC_c$KC_b$KC_a;-><init>(La/a/a/e/a/KC_c$KC_b;La/a/a/e/a/KC_c;)V

    new-instance v1, La/a/a/e/a/KC_e;

    invoke-direct {v1, v0}, La/a/a/e/a/KC_e;-><init>(La/a/a/e/a/KC_c$KC_b$KC_a;)V

    return-object v1
.end method
