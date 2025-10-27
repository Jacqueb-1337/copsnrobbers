.class final La/a/a/e/KC_i$KC_f$1;
.super Ljava/lang/Object;
.source "SourceFile"

# interfaces
.implements La/a/a/c/KC_b;


# annotations
.annotation system Ldalvik/annotation/EnclosingClass;
    value = La/a/a/e/KC_i$KC_f;
.end annotation

.annotation system Ldalvik/annotation/InnerClass;
    accessFlags = 0x8
    name = null
.end annotation

.annotation system Ldalvik/annotation/Signature;
    value = {
        "Ljava/lang/Object;",
        "La/a/a/c/KC_b",
        "<",
        "La/a/a/e/KC_i$KC_f;",
        ">;"
    }
.end annotation


# direct methods
.method constructor <init>()V
    .locals 0

    .prologue
    .line 1242
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method


# virtual methods
.method public final synthetic a(Landroid/os/Parcel;Ljava/lang/ClassLoader;)Ljava/lang/Object;
    .locals 1

    .prologue
    .line 1242
    new-instance v0, La/a/a/e/KC_i$KC_f;

    invoke-direct {v0, p1, p2}, La/a/a/e/KC_i$KC_f;-><init>(Landroid/os/Parcel;Ljava/lang/ClassLoader;)V

    return-object v0
.end method

.method public final bridge synthetic a(I)[Ljava/lang/Object;
    .locals 1

    .prologue
    .line 1242
    new-array v0, p1, [La/a/a/e/KC_i$KC_f;

    return-object v0
.end method
